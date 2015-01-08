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
    [Activity (Label = "FiredTV", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
	public class MainActivity : Activity
	{
		Timer timerUpdate;
		AppsAdapter adapter;
		GridView gridView;
		TextView textDate;
		TextView textTime;
        FrameLayout frameTopBar;
        ImageView wallpaper;
        int gridViewTopPadding = 0;
        string wallpaperFile = string.Empty;

        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			Console.WriteLine ("Is Amazon FireTV: " + Settings.IsFireTV ());

			RequestWindowFeature (WindowFeatures.NoTitle);
			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			textDate = FindViewById<TextView> (Resource.Id.textViewDate);
			textTime = FindViewById<TextView> (Resource.Id.textViewTime);
			gridView = FindViewById<GridView> (Resource.Id.gridView);
            frameTopBar = FindViewById<FrameLayout> (Resource.Id.frameTopBar);
            wallpaper = FindViewById<ImageView> (Resource.Id.imageWallpaper);

			StartService (new Intent (this, typeof(ExcuseMeService)));

			adapter = new AppsAdapter () { Context = this };

            gridViewTopPadding = gridView.PaddingTop;

			gridView.ItemClick += (sender, e) => {

				var app = adapter[e.Position];

				// If we're launching home, tell the service that checks
				// for intercepting this that it's ok
				if (app.PackageName == Settings.HOME_PACKAGE_NAME)
					ExcuseMeService.AllowFireTVHome = true;

				StartActivity (app.LaunchIntent);
			};

			gridView.Adapter = adapter;

			timerUpdate = new Timer (state => Setup (true), null, Timeout.Infinite, Timeout.Infinite);

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
			
        void Setup(bool dateOnly = false)
		{
			RunOnUiThread (() => {

				var timeFmt = "h:mm tt";
				if (Settings.Instance.TwentyFourHourTime)
					timeFmt = "H:mm";
				textTime.Text = DateTime.Now.ToString (timeFmt);
                textDate.Text = DateTime.Now.ToString ("dddd MMMM d").ToUpperInvariant ();

                if (dateOnly)
                    return;

                frameTopBar.Visibility = Settings.Instance.HideTopBar ? ViewStates.Gone : ViewStates.Visible;

                frameTopBar.SetBackgroundColor (new Android.Graphics.Color (0,0,0, Settings.Instance.TopInfoBarBackgroundAlpha));

                var pad = Settings.Instance.HideTopBar ? gridView.PaddingBottom : gridViewTopPadding;

                gridView.SetPadding (gridView.PaddingLeft, pad, gridView.PaddingRight, gridView.PaddingBottom);

                wallpaper.Visibility = Settings.Instance.WallpaperUse ? ViewStates.Visible : ViewStates.Gone;

                if (Settings.Instance.WallpaperUse) {
                
                    var filename = Settings.GetWallpaperFilename ();

                    if (!System.IO.File.Exists (filename)) {
                        filename = string.Empty;
                        wallpaperFile = string.Empty;
                    }

                    if (wallpaperFile != filename) {
                        wallpaperFile = filename;

                        if (string.IsNullOrEmpty (wallpaperFile)) {
                            wallpaper.SetImageResource (Settings.UseLargeTextures () ? Resource.Drawable.wallpaper : Resource.Drawable.wallpaper);
                        }
                        else {
                            try {
                                var drawable = Android.Graphics.Drawables.Drawable.CreateFromPath (filename);
                                wallpaper.SetImageDrawable (drawable);
                            } catch {
                                wallpaperFile = string.Empty;
                                wallpaper.SetImageResource (Settings.UseLargeTextures () ? Resource.Drawable.wallpaper : Resource.Drawable.wallpaper);
                            }
                        }
                    }                     
                }

			});
		}
	}
}
