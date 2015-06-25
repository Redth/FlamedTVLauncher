
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
using Android.Preferences;
using System.Threading.Tasks;
using AndroidHUD;

namespace FiredTVLauncher
{
	[Activity (Label = "FiredTV Settings")]			
	public class SettingsActivity : PreferenceActivity
	{
		Preference prefBlacklist;
		Preference prefReorder;
		CheckBoxPreference prefDisableHomeDetect;
        EditTextPreference prefWallpaperUrl;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

            Settings.Instance.Load ();

			// Create your application here
			AddPreferencesFromResource (Resource.Layout.Settings);

            prefBlacklist = FindPreference ("pref_blacklist");
            prefReorder = FindPreference ("pref_reorder");

			prefDisableHomeDetect = (CheckBoxPreference)FindPreference ("pref_disablecheck");
            prefWallpaperUrl = (EditTextPreference)FindPreference ("pref_WallpaperUrl");

			prefBlacklist.PreferenceClick += delegate {
				StartActivity (typeof (SettingsAppShowHideActivity));
			};
			prefReorder.PreferenceClick += delegate {
                StartActivity (typeof (ReorderActivity));
			};

            // Start the intent service, it will decide to stop itself or not
            prefDisableHomeDetect.PreferenceChange += (sender, e) => 
                StartService (new Intent (this, typeof(ExcuseMeService)));

            prefWallpaperUrl.PreferenceChange += (sender, e) => {
                AndHUD.Shared.Show(this, "Downloading Wallpaper...");

                var url = prefWallpaperUrl.EditText.Text;

                Task.Factory.StartNew (() => {

                    try {
                        var http = new System.Net.WebClient ();
                        var bytes = http.DownloadData (url);
                        var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                        var filename = System.IO.Path.Combine(path, "wallpaper.png");
                        System.IO.File.WriteAllBytes (filename, bytes);

                        AndHUD.Shared.Dismiss (this);
                    } catch (Exception ex) {

                        AndHUD.Shared.Dismiss (this);

                        Settings.Instance.WallpaperUrl = string.Empty;

                        Toast.MakeText (this, "Failed to Download Wallpaper", ToastLength.Long).Show ();
                        Log.Error ("Downloading Wallpaper Failed", ex);
                    }                        
                });
            };
		}            

        int ParseAlpha (string value)
        {
            var t = value;
            int num = 120;

            int.TryParse (t, out num);

            if (num < 0)
                num = 0;
            if (num > 255)
                num = 255;

            return num;
        }
	}
}

