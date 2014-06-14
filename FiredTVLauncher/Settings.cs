
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

		public bool HideFireTVApp { get; set; }
		public bool HideSettingsApp { get; set; }

		public bool HideLabels { get; set; }
		public int LabelFontSize { get; set; }

		public int HomeDetectIntervalMs { get; set; }
	}
}

