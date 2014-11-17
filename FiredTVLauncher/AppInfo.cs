using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Util;

namespace FiredTVLauncher
{

    public class AppInfo : Java.Lang.Object
    {
        public AppInfo()
        {

        }

        public Intent LaunchIntent { get; set; }
        public string Name { get; set; }
        public ApplicationInfo App { get; set; }

        public string PackageName { get; set; }

        public Drawable GetIcon(Context context)
        {
            Drawable icon = null;

            var metrics = new[]
                              {
                                  DisplayMetricsDensity.Xxxhigh, DisplayMetricsDensity.Xxhigh, DisplayMetricsDensity.Xhigh,
                                  DisplayMetricsDensity.Tv
                              };

            if (App != null)
            {
                foreach (var m in metrics)
                {
                    try
                    {
                        var pkgContext = context.CreatePackageContext(
                            App.PackageName,
                            PackageContextFlags.IgnoreSecurity);
                        icon = pkgContext.Resources.GetDrawableForDensity(App.Icon, (int)m);
                        break;
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                if (Name == "Settings")
                {
                    icon = context.Resources.GetDrawable(Resource.Drawable.settings);
                }
            }

            return icon;
        }

        public static void FetchApps(Context context, bool ignoreBlacklist, Action<List<AppInfo>> callback)
        {
            Task.Factory.StartNew(
                () =>
                    {
                        var apps = FetchApps(context, ignoreBlacklist);

                callback(apps);
            });
        }

        static List<AppInfo> FetchApps(Context context, bool ignoreBlacklist = false)
        {
            var results = new List<AppInfo>();
            var apps = context.PackageManager.GetInstalledApplications(PackageInfoFlags.Activities);

            foreach (var app in apps)
            {
                var launchIntent = context.PackageManager.GetLaunchIntentForPackage(app.PackageName);

                if (app.PackageName == Android.Provider.Settings.ActionSettings)
                {
                    Console.WriteLine("Settings");
                }

                if (launchIntent != null)
                {
                    if (!ignoreBlacklist && Settings.Instance.Blacklist.Contains(app.PackageName))
                    {
                        continue;
                    }

                    var label = app.LoadLabel(context.PackageManager);
                    if (app.PackageName == Settings.HOME_PACKAGE_NAME)
                    {
                        label = "FireTV";
                    }

                    results.Add(new AppInfo
                    {
                        LaunchIntent = launchIntent,
                        Name = label,
                        App = app,
                        PackageName = app.PackageName
                    });
                }
            }

            if (!Settings.Instance.Blacklist.Contains(Android.Provider.Settings.ActionSettings) || ignoreBlacklist)
            {
                results.Add(new AppInfo
                {
                    LaunchIntent = new Intent(Android.Provider.Settings.ActionSettings),
                    Name = "Settings",
                    App = null,
                    PackageName = Android.Provider.Settings.ActionSettings
                });
            }

            return results.OrderBy(x => x.Name).ToList();
        }
    }
}