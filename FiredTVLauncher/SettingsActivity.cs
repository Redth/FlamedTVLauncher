using System;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Widget;

using AndroidHUD;

namespace FiredTVLauncher
{
    [Activity(Label = "FiredTV Settings")]
    public class SettingsActivity : PreferenceActivity
    {
        Preference prefBlacklist;
        Preference prefReorder;
        EditTextPreference prefAppNameFontSize;
        CheckBoxPreference prefHideLabels;
        CheckBoxPreference prefHideTopBar;
        CheckBoxPreference prefHideDate;
        CheckBoxPreference prefHideTime;
        CheckBoxPreference prefTwentyFourHourTime;
        CheckBoxPreference prefDisableHomeDetect;
        EditTextPreference prefIconBackgroundAlpha;
        EditTextPreference prefLabelBackgroundAlpha;
        EditTextPreference prefTopInfoBarBackgroundAlpha;
        CheckBoxPreference prefWallpaperUse;
        EditTextPreference prefWallpaperUrl;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            AddPreferencesFromResource(Resource.Layout.Settings);

            prefBlacklist = FindPreference("pref_blacklist");
            prefReorder = FindPreference("pref_reorder");
            prefHideLabels = (CheckBoxPreference)FindPreference("pref_hidelabels");
            prefAppNameFontSize = (EditTextPreference)FindPreference("pref_applabelfontsize");
            prefHideTopBar = (CheckBoxPreference)FindPreference("pref_hidetopbar");
            prefHideDate = (CheckBoxPreference)FindPreference("pref_hidedate");
            prefHideTime = (CheckBoxPreference)FindPreference("pref_hidetime");
            prefTwentyFourHourTime = (CheckBoxPreference)FindPreference("pref_twentyfourhourtime");
            prefDisableHomeDetect = (CheckBoxPreference)FindPreference("pref_disablecheck");
            prefIconBackgroundAlpha = (EditTextPreference)FindPreference("pref_IconBackgroundAlpha");
            prefLabelBackgroundAlpha = (EditTextPreference)FindPreference("pref_LabelBackgroundAlpha");
            prefTopInfoBarBackgroundAlpha = (EditTextPreference)FindPreference("pref_TopInfoBarBackgroundAlpha");
            prefWallpaperUse = (CheckBoxPreference)FindPreference("pref_WallpaperUse");
            prefWallpaperUrl = (EditTextPreference)FindPreference("pref_WallpaperUrl");

            prefAppNameFontSize.EditText.InputType = Android.Text.InputTypes.ClassNumber;

            prefBlacklist.PreferenceClick += delegate
            {
                StartActivity(typeof(SettingsAppShowHideActivity));
            };
            prefReorder.PreferenceClick += delegate
            {
                StartActivity(typeof(ReorderActivity));
            };

            prefAppNameFontSize.PreferenceChange += SaveHandler;
            prefHideLabels.PreferenceChange += SaveHandler;
            prefHideTopBar.PreferenceChange += SaveHandler;
            prefHideDate.PreferenceChange += SaveHandler;
            prefHideTime.PreferenceChange += SaveHandler;
            prefTwentyFourHourTime.PreferenceChange += SaveHandler;
            prefDisableHomeDetect.PreferenceChange += SaveHandler;
            prefIconBackgroundAlpha.PreferenceChange += SaveHandler;
            prefLabelBackgroundAlpha.PreferenceChange += SaveHandler;
            prefTopInfoBarBackgroundAlpha.PreferenceChange += SaveHandler;
            prefWallpaperUse.PreferenceChange += SaveHandler;
            prefWallpaperUrl.PreferenceChange += SaveHandler;
        }

        void SaveHandler(object sender, Preference.PreferenceChangeEventArgs e)
        {
            if (sender == prefHideLabels)
                Settings.Instance.HideLabels = !prefHideLabels.Checked;
            if (sender == prefHideTopBar)
                Settings.Instance.HideTopBar = !prefHideTopBar.Checked;
            if (sender == prefHideDate)
                Settings.Instance.HideDate = !prefHideDate.Checked;
            if (sender == prefTwentyFourHourTime)
                Settings.Instance.TwentyFourHourTime = !prefTwentyFourHourTime.Checked;

            if (sender == prefHideTime)
                Settings.Instance.HideTime = !prefHideTime.Checked;

            if (sender == prefAppNameFontSize)
            {
                var size = 16;
                int.TryParse(prefAppNameFontSize.EditText.Text, out size);
                Settings.Instance.LabelFontSize = size;
            }

            if (sender == prefDisableHomeDetect)
            {
                Settings.Instance.DisableHomeDetection = !prefDisableHomeDetect.Checked;
                StartService(new Intent(this, typeof(ExcuseMeService)));
            }

            if (sender == prefIconBackgroundAlpha)
                Settings.Instance.IconBackgroundAlpha = ParseAlpha(prefIconBackgroundAlpha.EditText.Text);

            if (sender == prefLabelBackgroundAlpha)
                Settings.Instance.LabelBackgroundAlpha = ParseAlpha(prefLabelBackgroundAlpha.EditText.Text);

            if (sender == prefTopInfoBarBackgroundAlpha)
                Settings.Instance.TopInfoBarBackgroundAlpha = ParseAlpha(prefTopInfoBarBackgroundAlpha.EditText.Text);

            if (sender == prefWallpaperUse)
                Settings.Instance.WallpaperUse = prefWallpaperUse.Checked;

            if (sender == prefWallpaperUrl)
            {

                AndHUD.Shared.Show(this, "Downloading Wallpaper...");

                var url = prefWallpaperUrl.EditText.Text;

                Task.Factory.StartNew(() =>
                {

                    try
                    {
                        var http = new System.Net.WebClient();
                        var bytes = http.DownloadData(url);
                        var filename = Settings.GetWallpaperFilename();
                        System.IO.File.WriteAllBytes(filename, bytes);

                        Settings.Instance.WallpaperUrl = url;

                        Settings.Save();
                    }
                    catch (Exception ex)
                    {

                        Toast.MakeText(this, "Failed to Download Wallpaper", ToastLength.Long).Show();
                        Log.Error("Downloading Wallpaper Failed", ex);
                    }

                    AndHUD.Shared.Dismiss(this);
                });
            }

            Settings.Save();
        }

        int ParseAlpha(string value)
        {
            var t = value;
            int num = 120;

            int.TryParse(t, out num);

            if (num < 0)
                num = 0;
            if (num > 255)
                num = 255;

            return num;
        }
    }
}

