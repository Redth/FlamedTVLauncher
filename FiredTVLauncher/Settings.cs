
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
		}

		public List<string> Blacklist { get; set; }

		public bool HideLabels { get; set; }
		public int LabelFontSize { get; set; }

		public bool HideFiredTVLogo { get; set; }
		public bool HideDate { get;set; }
		public bool HideTime { get;set; }

		public int HomeDetectIntervalMs { get; set; }

		public static void Save ()
		{
			var path = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal), "settings.json");
			var json = new DataContractJsonSerializer (typeof(Settings));

			using (var sw = File.OpenWrite (path)) {
				json.WriteObject (sw, Settings.Instance);
			}
		}

		public static void Load ()
		{
			var path = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal), "settings.json");
			var json = new DataContractJsonSerializer (typeof(Settings));

			try {
				using (var sw = File.OpenRead (path)) {
					Settings.Instance = (Settings)json.ReadObject (sw);
				}
			} catch {
				Settings.Instance = new Settings ();
			}
		}
	}
}

