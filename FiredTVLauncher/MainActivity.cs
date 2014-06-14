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

namespace FiredTVLauncher
{
	[Activity (Label = "FiredTVLauncher", MainLauncher = true)]
	public class MainActivity : Activity
	{
		AppsAdapter adapter;
		GridView gridView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);


			FindViewById<ImageView> (Resource.Id.imageViewLogo).SetImageResource (Resource.Drawable.firedtv);

			gridView = FindViewById<GridView> (Resource.Id.gridView);
		

			StartService (new Intent (this, typeof(ExcuseMeService)));

			adapter = new AppsAdapter () { Context = this };

			gridView.ItemClick += (sender, e) => {
				var app = adapter[e.Position];

				// If we're launching home, tell the service that checks
				// for intercepting this that it's ok
				if (app.App.PackageName == Settings.HOME_PACKAGE_NAME)
					ExcuseMeService.AllowFireTVHome = true;

				StartActivity (app.LaunchIntent);
			};
			gridView.Adapter = adapter;

			adapter.Reload ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			// Tell the service to continue checking again now
			// that we've resumed our launcher
			ExcuseMeService.AllowFireTVHome = false;
		}
	}

	public class AppsAdapter : BaseAdapter<AppInfo>
	{
		public Context Context { get; set; }

		List<AppInfo> apps = new List<AppInfo> ();

		public override long GetItemId (int position) { return position; } 
		public override int Count { get { return apps.Count; } }
		public override AppInfo this [int index] { get { return apps [index]; } }

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var app = apps [position];
			Android.Graphics.Drawables.Drawable draw = null;

			var name = app.Name;
			if (app.App != null) {
				draw = app.App.LoadIcon (Context.PackageManager);
			} else {
				if (name == "Settings")
					draw = Context.Resources.GetDrawable (Resource.Drawable.settings);
			}

			var view = convertView ??
				LayoutInflater.FromContext (Context).Inflate (Resource.Layout.GridItemLayout, parent, false);

			var label = view.FindViewById<TextView> (Resource.Id.textName);
			label.Text = app.Name;
			label.TextSize = Settings.Instance.LabelFontSize;

			if (Settings.Instance.HideLabels)
				label.Visibility = ViewStates.Gone;


			var icon = view.FindViewById<ImageView> (Resource.Id.imageIcon);

			if (draw != null)
				icon.SetImageDrawable (draw);

			return view;
		}

		public void Reload () 
		{
			var r = AppInfo.FetchApps (Context);

			apps.Clear ();
			apps.AddRange (r);

			NotifyDataSetChanged ();
		}
	}

	public class AppInfo : Java.Lang.Object
	{
		public Intent LaunchIntent { get;set; }
		public string Name { get; set; }
		public ApplicationInfo App { get; set; }

		public static List<AppInfo> FetchApps (Context context)
		{
			var results = new List<AppInfo> ();
			var apps = context.PackageManager.GetInstalledApplications (PackageInfoFlags.Activities);

			foreach (var app in apps) {

				var launchIntent = context.PackageManager.GetLaunchIntentForPackage (app.PackageName);

				if (launchIntent != null) {

					if (Settings.Instance.Blacklist.Contains (app.PackageName))
						continue;

					var label = app.LoadLabel (context.PackageManager);
					if (app.PackageName == Settings.HOME_PACKAGE_NAME) {
						if (Settings.Instance.HideFireTVApp)
							continue;
						label = "FireTV";
					}

					Console.WriteLine ("Found: " + app.PackageName);

					results.Add (new AppInfo {
						LaunchIntent = launchIntent,
						Name = label,
						App = app
					});
				}
			}

			if (!Settings.Instance.HideSettingsApp) {
				results.Add (new AppInfo {
					LaunchIntent = new Intent (Android.Provider.Settings.ActionSettings),
					Name = "Settings",
					App = null
				});
			}

			return results;
		}
	}
}


