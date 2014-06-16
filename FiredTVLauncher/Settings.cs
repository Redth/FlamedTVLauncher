
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

namespace FiredTVLauncher
{
	public class Settings
	{
		public const string HOME_PACKAGE_NAME = "com.amazon.tv.launcher";
		public const string HOME_CLASS_NAME = "com.amazon.tv.launcher.ui.HomeActivity";

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

			if (Blacklist.Count <= 0) {
				Blacklist.Add ("com.altusapps.firedtvlauncher");
				Blacklist.Add ("com.amazon.avod");
				Blacklist.Add ("com.amazon.bueller.photos");
				Blacklist.Add ("com.amazon.device.bluetoothdfu");
				Blacklist.Add ("com.amazon.device.gmo");
				Blacklist.Add ("com.amazon.venezia");				
			}

			HideLabels = false;
			LabelFontSize = 18;
			TwentyFourHourTime = false;
		}

		public List<AppOrder> Ordering { get; set; }

		public List<string> Blacklist { get; set; }

		public bool HideLabels { get; set; }
		public int LabelFontSize { get; set; }

		public bool HideFiredTVLogo { get; set; }
		public bool HideDate { get;set; }
		public bool HideTime { get;set; }
		public bool TwentyFourHourTime { get;set; }

		public int HomeDetectIntervalMs { get; set; }

		public void SanitizeAppOrder(List<AppInfo> apps)
		{
			if (apps != null) {
				foreach (var app in apps) {
					GetAppOrder (app.PackageName);
				}
			}

			Ordering.Sort ((o1, o2) => o1.Order.CompareTo (o2.Order));

			var i = 0;
			foreach (var app in Ordering)
				app.Order = i++;

			Save ();
		}

		public int GetAppOrder (string packageName)
		{
			var app = Ordering.FirstOrDefault (ao => ao.PackageName == packageName);
			if (app == null) {
				var newOrder = 0;
				if (Ordering.Any ())
					newOrder = Ordering.Max (ao => ao.Order) + 1;

				app = new AppOrder { PackageName = packageName, Order = newOrder };
				Ordering.Add (app);

				Save ();
			}

			return app.Order;
		}

		public int MoveAppOrder (string packageName, int currentOrder, int newOrder)
		{
			var currentAppOrder = Ordering.FirstOrDefault (ao => ao.PackageName == packageName);

			if (currentAppOrder == null)
				return -1;

			if (newOrder < currentOrder) {
				foreach (var appOrder in Ordering) {

					if (appOrder.Order >= newOrder && appOrder.PackageName != packageName)
						appOrder.Order++;
				}
			} else if (newOrder > currentOrder) {
				foreach (var appOrder in Ordering) {
					if (appOrder.Order >= currentOrder && appOrder.Order <= newOrder && appOrder.PackageName != packageName) {
						appOrder.Order--; 
					}
				}
			}

			currentAppOrder.Order = newOrder;

			SanitizeAppOrder (null);

			return newOrder;
		}

		public static void Save ()
		{
			var path = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments), "settings.json");
			var json = new DataContractJsonSerializer (typeof(Settings));

			using (var sw = File.OpenWrite (path)) {
				json.WriteObject (sw, Settings.Instance);
			}
		}

		public static void Load ()
		{
			var path = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments), "settings.json");
			var json = new DataContractJsonSerializer (typeof(Settings));

			try {
				using (var sw = File.OpenRead (path)) {
					Settings.Instance = (Settings)json.ReadObject (sw);
				}
			} catch {
				Settings.Instance = new Settings ();
			}
		}

		public static bool IsFireTV () {

			var manu = Android.OS.Build.Manufacturer;
			var model = Android.OS.Build.Model;

			return manu.Equals ("Amazon") && model.StartsWith ("AFT", StringComparison.InvariantCultureIgnoreCase);
		}
	}
}

