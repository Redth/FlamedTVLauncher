using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Content.PM;
using Android.Content.Res;
using System.Threading;

namespace FiredTVLauncher
{
	[IntentFilter (new [] { Android.Content.Intent.ActionMain }, Categories=new [] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryHome })]
	[Activity (Label = "FiredTV")]
	public class MainActivity : Activity
	{

		Timer timerUpdate;
		AppsAdapter adapter;
		GridView gridView;
		TextView textDate;
		TextView textTime;
		ImageView imageLogo;
        View dividerLine;
		
        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			Console.WriteLine ("Is Amazon FireTV: " + Settings.IsFireTV ());

			RequestWindowFeature (WindowFeatures.NoTitle);
			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			textDate = FindViewById<TextView> (Resource.Id.textViewDate);
			textTime = FindViewById<TextView> (Resource.Id.textViewTime);
			imageLogo = FindViewById<ImageView> (Resource.Id.imageViewLogo);
            dividerLine = FindViewById<View> (Resource.Id.dividerLine);

			imageLogo.SetImageResource (Resource.Drawable.firedtv);

			gridView = FindViewById<GridView> (Resource.Id.gridView);
		

			StartService (new Intent (this, typeof(ExcuseMeService)));

			adapter = new AppsAdapter () { Context = this };

			gridView.ItemClick += (sender, e) => {

				var app = adapter[e.Position];

				// If we're launching home, tell the service that checks
				// for intercepting this that it's ok
				if (app.PackageName == Settings.HOME_PACKAGE_NAME)
					ExcuseMeService.AllowFireTVHome = true;

				StartActivity (app.LaunchIntent);
			};

			gridView.Adapter = adapter;

			timerUpdate = new Timer (state => Setup (), null, Timeout.Infinite, Timeout.Infinite);

            RegisterForContextMenu (gridView);
		}

		protected override void OnResume ()
		{
			base.OnResume ();

            Settings.Load ();

			Setup ();

			// Tell the service to continue checking again now
			// that we've resumed our launcher
			ExcuseMeService.AllowFireTVHome = false;

			adapter.Reload ();

			textDate.Visibility = Settings.Instance.HideDate ? ViewStates.Gone : ViewStates.Visible;
			textTime.Visibility = Settings.Instance.HideTime ? ViewStates.Gone : ViewStates.Visible;
			imageLogo.Visibility = Settings.Instance.HideFiredTVLogo ? ViewStates.Gone : ViewStates.Visible;
            dividerLine.Visibility = Settings.Instance.HideHomeDividerLine ? ViewStates.Gone : ViewStates.Visible;

			timerUpdate.Change (TimeSpan.FromSeconds (10), TimeSpan.FromSeconds (10));


		}

		protected override void OnPause ()
		{
			base.OnPause ();

			timerUpdate.Change (Timeout.Infinite, Timeout.Infinite);
		}


		public override bool OnPrepareOptionsMenu (IMenu menu)
		{

			return false;
		}


        public override void OnCreateContextMenu (IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu (menu, v, menuInfo);


            menu.Add (0, 0, 0, "Hide App");
            menu.Add (0, 1, 1, "Uninstall App");
        }

        public override bool OnContextItemSelected (IMenuItem item)
        {
            var info = (AdapterView.AdapterContextMenuInfo) item.MenuInfo;

            var app = adapter [info.Position];

            if (item.ItemId == 0) {
                // Hide
                Settings.Instance.Blacklist.Add (app.PackageName);
                Settings.Save ();
                adapter.Reload ();
                Toast.MakeText (this, "Hiding " + app.Name, ToastLength.Short).Show ();
            } else if (item.ItemId == 1) {
                // Uninstall
                var packageUri = Android.Net.Uri.Parse("package:" + app.PackageName);
                var uninstallIntent = new Intent(Intent.ActionDelete, packageUri);
                StartActivity (uninstallIntent);
            }
            return base.OnContextItemSelected (item);
        }
		public override bool OnKeyDown (Keycode keyCode, KeyEvent e)
		{
			if (keyCode == Keycode.Menu) {
				StartActivity (typeof(SettingsActivity));
				return true;
			}
					
			return base.OnKeyDown (keyCode, e);
		}
			
		void Setup()
		{
			RunOnUiThread (() => {
				var timeFmt = "h:mm tt";
				if (Settings.Instance.TwentyFourHourTime)
					timeFmt = "H:mm";
				textTime.Text = DateTime.Now.ToString (timeFmt);
				textDate.Text = DateTime.Now.ToString ("dddd MMMM d");
			});
		}

	}
}


