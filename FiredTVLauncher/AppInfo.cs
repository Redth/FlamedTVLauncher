using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Content.PM;
using Android.Content.Res;
using System.Threading.Tasks;
using Android.Graphics.Drawables;

namespace FiredTVLauncher
{

	public class AppInfo : Java.Lang.Object
	{
		public AppInfo ()
		{
		
		}

		public Intent LaunchIntent { get;set; }
		public string Name { get; set; }
		public ApplicationInfo App { get; set; }

		public string PackageName { get; set; }

		public Drawable GetIcon (Context context)
		{
			Drawable icon = null;

			if (App != null) {
				icon = App.LoadIcon (context.PackageManager);
			} else {
				if (Name == "Settings")
					icon = context.Resources.GetDrawable (Resource.Drawable.settings);
			}

			return icon;
		}

		public static void FetchApps (Context context, bool ignoreBlacklist, Action<List<AppInfo>> callback)
		{
			Task.Factory.StartNew (() => {

				var apps = FetchApps (context, ignoreBlacklist);

				callback (apps);
			});
		}

		static List<AppInfo> FetchApps (Context context, bool ignoreBlacklist = false)
		{
			var results = new List<AppInfo> ();
			var apps = context.PackageManager.GetInstalledApplications (PackageInfoFlags.Activities);

			foreach (var app in apps) {

				var launchIntent = context.PackageManager.GetLaunchIntentForPackage (app.PackageName);

				if (app.PackageName == Android.Provider.Settings.ActionSettings)
					Console.WriteLine ("Settings");

				if (launchIntent != null) {

					if (!ignoreBlacklist && Settings.Instance.Blacklist.Contains (app.PackageName))
						continue;

					var label = app.LoadLabel (context.PackageManager);
					if (app.PackageName == Settings.HOME_PACKAGE_NAME)
						label = "FireTV";

					results.Add (new AppInfo {
						LaunchIntent = launchIntent,
						Name = label,
						App = app,
						PackageName = app.PackageName
					});
				}
			}

			if (!Settings.Instance.Blacklist.Contains (Android.Provider.Settings.ActionSettings) || ignoreBlacklist) {
				results.Add (new AppInfo {
					LaunchIntent = new Intent (Android.Provider.Settings.ActionSettings),
					Name = "Settings",
					App = null,
					PackageName = Android.Provider.Settings.ActionSettings
				});
			}

			return results.OrderBy(x => x.Name).ToList ();
		}
	}
}