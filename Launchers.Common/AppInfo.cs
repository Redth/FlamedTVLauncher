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
using Android.Util;

namespace Launchers.Common
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

        public Drawable GetIcon (Context context, Dictionary<string, int> overrideIcons = null)
		{
            if (overrideIcons == null)
                overrideIcons = new Dictionary<string, int> ();

			Drawable icon = null;
           
            var metrics = new List<DisplayMetricsDensity> {
                //DisplayMetricsDensity.Xxxhigh,
                DisplayMetricsDensity.Xxhigh,
                DisplayMetricsDensity.Xhigh,
                DisplayMetricsDensity.Tv
            };

            if (App != null) {

                var pkgContext = context.CreatePackageContext (App.PackageName, PackageContextFlags.IgnoreSecurity);

                // Try and find the override first
                if (overrideIcons.ContainsKey (App.PackageName)) {
                    icon = context.Resources.GetDrawable (overrideIcons [App.PackageName]);
                } else {
                    // otherwise, go through the densities one by one
                    foreach (var m in metrics) {
                        try {
                            icon = pkgContext.Resources.GetDrawableForDensity (App.Icon, (int)m);
                            break;
                        } catch {
                            continue;
                        }
                    }
                }

            } else {
                if (overrideIcons.ContainsKey (PackageName))
                    icon = context.Resources.GetDrawable (overrideIcons [PackageName]);
            }

			return icon;
		}



        public static void FetchApps (Context context, List<string> ignoredPackageNames, bool addSettings, Dictionary<string, string> renameMappings, Action<List<AppInfo>> callback)
		{
			Task.Factory.StartNew (() => {

				var apps = FetchApps (context, addSettings, ignoredPackageNames, renameMappings);

				callback (apps);
			});
		}

        static List<AppInfo> FetchApps (Context context, bool addSettings = false, List<string> ignoredPackageNames = null, Dictionary<string, string> renameMappings = null)
		{
            if (renameMappings == null)
                renameMappings = new Dictionary<string, string> ();
            if (ignoredPackageNames == null)
                ignoredPackageNames = new List<string> ();

			var results = new List<AppInfo> ();
			var apps = context.PackageManager.GetInstalledApplications (PackageInfoFlags.Activities);

			foreach (var app in apps) {

				var launchIntent = context.PackageManager.GetLaunchIntentForPackage (app.PackageName);

				if (launchIntent != null) {

					if (ignoredPackageNames.Contains (app.PackageName))
						continue;
                        
					var label = app.LoadLabel (context.PackageManager);
                    if (renameMappings.ContainsKey (app.PackageName))
                        label = renameMappings [app.PackageName];
					
					results.Add (new AppInfo {
						LaunchIntent = launchIntent,
						Name = label,
						App = app,
						PackageName = app.PackageName
					});
				}
			}

			if (!ignoredPackageNames.Contains (Android.Provider.Settings.ActionSettings) && addSettings) {
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