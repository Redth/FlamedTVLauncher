
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
		EditTextPreference prefAppNameFontSize;
		CheckBoxPreference prefHideLabels;
		CheckBoxPreference prefHideLogo;
		CheckBoxPreference prefHideDate;
		CheckBoxPreference prefHideTime;


		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here
			AddPreferencesFromResource (Resource.Layout.Settings);

			prefBlacklist = FindPreference ("pref_blacklist");
			prefHideLabels = (CheckBoxPreference)FindPreference ("pref_hidelabels");
			prefAppNameFontSize = (EditTextPreference)FindPreference ("pref_applabelfontsize");
			prefHideLogo = (CheckBoxPreference)FindPreference ("pref_hidelogo");
			prefHideDate = (CheckBoxPreference)FindPreference ("pref_hidedate");
			prefHideTime = (CheckBoxPreference)FindPreference ("pref_hidetime");

			prefAppNameFontSize.EditText.InputType = Android.Text.InputTypes.ClassNumber;

			prefBlacklist.PreferenceClick += delegate {
				StartActivity (typeof (SettingsAppShowHideActivity));
			};
				
			prefAppNameFontSize.PreferenceChange += SaveHandler;
			prefHideLabels.PreferenceChange += SaveHandler;
			prefHideLogo.PreferenceChange += SaveHandler;
			prefHideDate.PreferenceChange += SaveHandler;
			prefHideTime.PreferenceChange += SaveHandler;
		}


		void SaveHandler (object sender, Preference.PreferenceChangeEventArgs e)
		{

			if (sender == prefHideLabels)
				Settings.Instance.HideLabels = !prefHideLabels.Checked;
			if (sender == prefHideLogo)
				Settings.Instance.HideFiredTVLogo = !prefHideLogo.Checked;
			if (sender == prefHideDate)
				Settings.Instance.HideDate = !prefHideDate.Checked;

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

