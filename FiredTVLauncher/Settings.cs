
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
			Ordering = new List<AppOrder> ();
		}

        object orderingLockObj = new object();

		public List<AppOrder> Ordering { get; set; }

		public List<string> Blacklist { 
            get {
                return getPrefs ().GetStringSet ("prefsBlacklist", new List<string> {
                    "com.altusapps.firedtvlauncher",
                    "com.amazon.avod",
                    "com.amazon.bueller.photos",
                    "com.amazon.device.bluetoothdfu",
                    "com.amazon.device.gmo",
                    "com.amazon.venezia",
                    "com.amazon.storm.lightning.tutorial",
                    "com.broadcom.wfd.client"
                }).ToList ();
            } 
            set {
                editPrefs ().PutStringSet ("prefsBlacklist", value).Commit ();
            }
        }

        public bool HideTopBar { 
            get { return getPrefs ().GetBoolean ("pref_hidetopbar", false); }
            set { editPrefs ().PutBoolean ("pref_hidetopbar", value).Commit (); }
        }

		public bool HideLabels { 
            get { return getPrefs ().GetBoolean ("pref_hidelabels", false); }
            set { editPrefs ().PutBoolean ("pref_hidelabels", value).Commit (); }
        }
		public int LabelFontSize { 
            get { return getInt ("pref_applabelfontsize", 18); }
            set { putInt ("pref_applabelfontsize", value); }
        }

		public bool HideDate { 
            get { return getPrefs ().GetBoolean ("pref_hidedate", false); }
            set { editPrefs ().PutBoolean ("pref_hidedate", value).Commit (); }
        }
		public bool HideTime {
            get { return getPrefs ().GetBoolean ("pref_hidetime", false); }
            set { editPrefs ().PutBoolean ("pref_hidetime", value).Commit (); }
        }
		public bool TwentyFourHourTime {
            get { return getPrefs ().GetBoolean ("pref_twentyfourhourtime", false); }
            set { editPrefs ().PutBoolean ("pref_twentyfourhourtime", value).Commit (); }
        }

        public int IconBackgroundAlpha { 
            get { return getInt ("pref_IconBackgroundAlpha", 120); }
            set { putInt ("pref_IconBackgroundAlpha", value); }
        }
        public int TopInfoBarBackgroundAlpha {
            get { return getInt ("pref_TopInfoBarBackgroundAlpha", 120); }
            set { putInt ("pref_TopInfoBarBackgroundAlpha", value); }
        }
        public int LabelBackgroundAlpha {
            get { return getInt ("pref_LabelBackgroundAlpha", 200); }
            set { putInt ("pref_LabelBackgroundAlpha", value); }
        }

		public int HomeDetectIntervalMs {
            get { return getInt ("pref_HomeDetectIntervalMs", 700); }
            set { putInt ("pref_HomeDetectIntervalMs", value); }
        }

		public bool DisableHomeDetection {
            get { return getPrefs ().GetBoolean ("pref_disablecheck", false); }
            set { editPrefs ().PutBoolean ("pref_disablecheck", value).Commit (); }
        }

        public bool WallpaperUse {
            get { return getPrefs ().GetBoolean ("pref_WallpaperUse", false); }
            set { editPrefs ().PutBoolean ("pref_WallpaperUse", value).Commit (); }
        }
        public string WallpaperUrl { 
            get { return getPrefs ().GetString ("pref_WallpaperUrl", string.Empty); }
            set { editPrefs ().PutString ("pref_WallpaperUrl", value).Commit (); }
        }

        int getInt (string key, int defaultValue)
        {
            var s = getPrefs ().GetString (key, defaultValue.ToString ());

            int i = defaultValue;
            int.TryParse (s, out i);

            return i;
        }

        void putInt (string key, int value)
        {
            editPrefs ().PutString (key, value.ToString ()).Commit ();
        }

        ISharedPreferences getPrefs ()
        {
            return Android.Preferences.PreferenceManager.GetDefaultSharedPreferences (Application.Context);
        }

        ISharedPreferencesEditor editPrefs ()
        {
            var prefs = Android.Preferences.PreferenceManager.GetDefaultSharedPreferences (Application.Context);
            return prefs.Edit ();
        }

		/**
		 * Return the default wallpaper path
		 */
		public static string GetWallpaperPath()
		{
			var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			return System.IO.Path.Combine(path, "wallpaper.png");
		}
			

        public static string GetWallpaperFilename() 
        {
            try {                
				var filename = Settings.GetWallpaperPath();

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

            lock (orderingLockObj) {
                Ordering.Sort ((o1, o2) => o1.Order.CompareTo (o2.Order));

                var i = 1;
                foreach (var app in Ordering)
                    app.Order = i++;
            }

			Save ();
		}

        public AppOrder GetAppOrder (string packageName)
        {
            lock (orderingLockObj) {
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
        }

        public void MoveOrder (string packageName, bool up)
        {
            var order = GetAppOrder (packageName);

            lock (orderingLockObj) {
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
            }

            Save ();
        }

		public void Save ()
		{
            var appOrders = new List<string> ();

            lock (orderingLockObj) {
                foreach (var o in Ordering) {
                    var s = string.Format ("{0}|{1}", o.Order, o.PackageName);
                    appOrders.Add (s);
                }
            }

            try { 
                editPrefs ().PutStringSet ("prefsAppOrder", appOrders).Commit ();
            } catch (Exception ex) {
                Console.WriteLine ("Save order exception: " + ex);
            }
		}

        public void Load ()
		{
            var appOrders = getPrefs ().GetStringSet ("prefsAppOrder", new List<string> ()).ToList ();

            var ordering = new List<AppOrder> ();


                foreach (var s in appOrders) {
                    var parts = s.Split (new char [] { '|' }, 2);

                    if (parts.Length == 2) {
                        int order = 0;
                        if (int.TryParse (parts [0], out order)) {
                            ordering.Add (new AppOrder {
                                Order = order,
                                PackageName = parts [1]
                            });
                        }
                    }
                }

            ordering.Sort ((o1, o2) => o1.Order.CompareTo (o2.Order));

            lock (orderingLockObj) {
                Ordering.Clear ();
                Ordering.AddRange (ordering);
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

