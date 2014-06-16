
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

namespace FiredTVLauncher
{
	[Activity (Label = "FiredTV Settings")]			
	public class SettingsActivity : PreferenceActivity
	{
		Preference prefBlacklist;
		Preference prefReorder;

		EditTextPreference prefAppNameFontSize;
		CheckBoxPreference prefHideLabels;
		CheckBoxPreference prefHideLogo;
		CheckBoxPreference prefHideDate;
		CheckBoxPreference prefHideTime;
		CheckBoxPreference prefTwentyFourHourTime;


		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here
			AddPreferencesFromResource (Resource.Layout.Settings);

			prefBlacklist = FindPreference ("pref_blacklist");
			prefReorder = FindPreference ("pref_reorder");
			prefHideLabels = (CheckBoxPreference)FindPreference ("pref_hidelabels");
			prefAppNameFontSize = (EditTextPreference)FindPreference ("pref_applabelfontsize");
			prefHideLogo = (CheckBoxPreference)FindPreference ("pref_hidelogo");
			prefHideDate = (CheckBoxPreference)FindPreference ("pref_hidedate");
			prefHideTime = (CheckBoxPreference)FindPreference ("pref_hidetime");
			prefTwentyFourHourTime = (CheckBoxPreference)FindPreference ("pref_twentyfourhourtime");

			prefAppNameFontSize.EditText.InputType = Android.Text.InputTypes.ClassNumber;

			prefBlacklist.PreferenceClick += delegate {
				StartActivity (typeof (SettingsAppShowHideActivity));
			};
			prefReorder.PreferenceClick += delegate {
				AlertDialog dlg = null;
				var bld = new AlertDialog.Builder(this);

				bld.SetTitle ("Re-Order Apps");
				bld.SetMessage ("To re-order apps on the home screen, select the app you want to re-order, then long click the item.  This will put the app into re-order mode.  You can now move the app around until you are happy with its position, and select the item again once to exit re-order mode");
				bld.SetNegativeButton("OK", delegate {
					dlg.Dismiss();
				});

				dlg = bld.Create();
				dlg.Show();
			};

			prefAppNameFontSize.PreferenceChange += SaveHandler;
			prefHideLabels.PreferenceChange += SaveHandler;
			prefHideLogo.PreferenceChange += SaveHandler;
			prefHideDate.PreferenceChange += SaveHandler;
			prefHideTime.PreferenceChange += SaveHandler;
			prefTwentyFourHourTime.PreferenceChange += SaveHandler;
		}


		void SaveHandler (object sender, Preference.PreferenceChangeEventArgs e)
		{

			if (sender == prefHideLabels)
				Settings.Instance.HideLabels = !prefHideLabels.Checked;
			if (sender == prefHideLogo)
				Settings.Instance.HideFiredTVLogo = !prefHideLogo.Checked;
			if (sender == prefHideDate)
				Settings.Instance.HideDate = !prefHideDate.Checked;
			if (sender == prefTwentyFourHourTime)
				Settings.Instance.TwentyFourHourTime = !prefTwentyFourHourTime.Checked;

			if (sender == prefHideTime)	
				Settings.Instance.HideTime = !prefHideTime.Checked;

			if (sender == prefAppNameFontSize) {
				var size = 16;
				int.TryParse (prefAppNameFontSize.EditText.Text, out size);
				Settings.Instance.LabelFontSize = size;
			}

			Settings.Save ();

		}
	}
}

