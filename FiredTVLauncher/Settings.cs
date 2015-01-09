
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Runtime.Serialization.Json;
using System.IO;
using Launchers.Common;

namespace FiredTVLauncher
{
	public class Settings
	{
		public const string HOME_PACKAGE_NAME = "com.amazon.tv.launcher";
		public const string HOME_CLASS_NAME = "com.amazon.tv.launcher.ui.HomeActivity";

        public static readonly Dictionary<string, int> ICON_OVERRIDES = new Dictionary<string, int> {
            { Android.Provider.Settings.ActionSettings, Resource.Drawable.settings },
            { Settings.HOME_PACKAGE_NAME, Resource.Drawable.firetvicon },           
        };

        public static readonly Dictionary<string, string> RENAME_MAPPINGS = new Dictionary<string, string> {
            { Settings.HOME_PACKAGE_NAME, "FireTV Home" }
        };

		static Settings()
		{
			Instance = new Settings ();
		}

		public static Settings Instance { get; set; }
		public Settings ()
		{
			HomeDetectIntervalMs = 700;

			Blacklist = new List<string> ();
			Ordering = new List<AppOrder> ();

			HideLabels = false;
			LabelFontSize = 18;
			TwentyFourHourTime = false;

            IconBackgroundAlpha = 120;
            LabelBackgroundAlpha = 200;
            TopInfoBarBackgroundAlpha = 120;

            WallpaperUrl = "Default";
            WallpaperUse = true;

            DisableHomeDetection = true;
		}

		public List<AppOrder> Ordering { get; set; }

		public List<string> Blacklist { get; set; }

        public bool HideTopBar { get; set; }
		public bool HideLabels { get; set; }
		public int LabelFontSize { get; set; }

		public bool HideDate { get;set; }
		public bool HideTime { get;set; }
		public bool TwentyFourHourTime { get;set; }

        public int IconBackgroundAlpha { get;set; }
        public int TopInfoBarBackgroundAlpha { get;set; }
        public int LabelBackgroundAlpha { get;set; }

		public int HomeDetectIntervalMs { get; set; }

		public bool DisableHomeDetection { get;set; }

        public bool WallpaperUse { get;set; }
        public string WallpaperUrl { get;set; }


        public static string GetWallpaperFilename() 
        {
            try {
                var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                var filename = System.IO.Path.Combine(path, "wallpaper.png");

                if (File.Exists (filename))
                    return filename;
            } catch {
            }

            return null;
        }


		public void SanitizeAppOrder(List<AppInfo> apps)
		{
			if (apps != null) {
				foreach (var app in apps) {
					GetAppOrder (app.PackageName);
				}
			}

			Ordering.Sort ((o1, o2) => o1.Order.CompareTo (o2.Order));

			var i = 1;
			foreach (var app in Ordering)
				app.Order = i++;

			Save ();
		}

        public AppOrder GetAppOrder (string packageName)
        {
            var order = Ordering.FirstOrDefault (ao => ao.PackageName.Equals (packageName));

            // Make sure the current ordering actually exists
            if (order == null) {
                var index = 1;
                // If it doesn't exist, let's assume last in line
                var after = Ordering.LastOrDefault ();
                if (after != null)
                    index = after.Order + 1;

                // Make our order
                order = new AppOrder {
                    PackageName = packageName,
                    Order = index
                };

                // Order didn't exist so let's add it
                Ordering.Add (order);
            }

            return order;
        }

        public void MoveOrder (string packageName, bool up)
        {
            var order = GetAppOrder (packageName);

            // Can only go so far up
            if (up && order.Order <= 1)
                return;

            if (!up && order.Order >= Ordering.Count) // Ordering.Last ().PackageName.Equals (packageName))
                return;

            if (up)
                order.Order = order.Order - 1;
            else
                order.Order = order.Order + 1;

            foreach (var appOrder in Ordering) {
                if (appOrder.Order == order.Order
                    && !appOrder.PackageName.Equals (order.PackageName)) {

                    if (up)
                        appOrder.Order = appOrder.Order + 1;
                    else
                        appOrder.Order = appOrder.Order - 1;
                }
            }

            Save ();
        }

		public static void Save ()
		{
			var path = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments), "settings2.json");
            try {
                File.WriteAllText (path, Newtonsoft.Json.JsonConvert.SerializeObject (Settings.Instance));
                Log.Debug ("Settings Saved to {0}", path);
            } catch (Exception ex) {
                Log.Error ("Failed to write settings file", ex);
            }
		}

        public static void Load ()
		{
			var path = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments), "settings2.json");

            try {
                Settings.Instance = Newtonsoft.Json.JsonConvert.DeserializeObject <Settings> (File.ReadAllText (path));
                Log.Debug ("Settings Loaded from {0}", path);
            }
            catch (Exception ex) {
                Log.Error ("Failed to load settings file", ex);
                Settings.Instance = new Settings ();

                if (Settings.Instance.Blacklist.Count <= 0) {
                    Settings.Instance.Blacklist.Add ("com.altusapps.firedtvlauncher");
                    Settings.Instance.Blacklist.Add ("com.amazon.avod");
                    Settings.Instance.Blacklist.Add ("com.amazon.bueller.photos");
                    Settings.Instance.Blacklist.Add ("com.amazon.device.bluetoothdfu");
                    Settings.Instance.Blacklist.Add ("com.amazon.device.gmo");
                    Settings.Instance.Blacklist.Add ("com.amazon.venezia");   
                    Settings.Instance.Blacklist.Add ("com.amazon.storm.lightning.tutorial");
                    Settings.Instance.Blacklist.Add ("com.broadcom.wfd.client");
                }
            }
		}

		public static bool IsFireTV () {

			var manu = Build.Manufacturer;
			var model = Build.Model;

			return manu.Equals ("Amazon") && model.StartsWith ("AFT", StringComparison.InvariantCultureIgnoreCase);
		}            

        public static bool UseLargeTextures () 
        {
            var c = new Android.Graphics.Canvas ();
            return c.MaximumBitmapWidth > 2048;
        }
	}
}

