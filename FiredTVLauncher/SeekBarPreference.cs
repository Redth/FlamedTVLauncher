using System;
using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Preferences;
using Android.Views;

namespace FiredTVLauncher
{
    public class SeekBarPreference : Preference, Android.Widget.SeekBar.IOnSeekBarChangeListener
    {
        const string ANDROIDNS = "http://schemas.android.com/apk/res/android";
        const string APPLICATIONNS = "http://firedtvlauncher.com";
        const int DEFAULT_VALUE = 50;


        int maxValue      = 100;
        int minValue      = 0;
        int interval      = 1;
        int currentValue;
        string unitsLeft  = "";
        string unitsRight = "";
        SeekBar seekBar;

        TextView statusText;

        public SeekBarPreference (Context context, IAttributeSet attrs) : base (context, attrs)
        {        
            InitPreference (context, attrs);
        }

        public SeekBarPreference(Context context, IAttributeSet attrs, int defStyle) : base (context, attrs, defStyle)
        {
            InitPreference (context, attrs);
        }

        private void InitPreference(Context context, IAttributeSet attrs) 
        {
            SetValuesFromXml(attrs);

            seekBar = new SeekBar (context, attrs);
            seekBar.Max = maxValue - minValue;
            seekBar.SetOnSeekBarChangeListener (this);

            this.WidgetLayoutResource = Resource.Layout.seek_bar_preference;
        }

        private void SetValuesFromXml(IAttributeSet attrs) 
        {
            maxValue = attrs.GetAttributeIntValue (ANDROIDNS, "max", 100);
            minValue = attrs.GetAttributeIntValue (APPLICATIONNS, "min", 0);

            unitsLeft = GetAttributeStringValue (attrs, APPLICATIONNS, "unitsLeft", "");
            var units = GetAttributeStringValue (attrs, APPLICATIONNS, "units", "");
            unitsRight = GetAttributeStringValue (attrs, APPLICATIONNS, "unitsRight", units);

            var newInterval = attrs.GetAttributeValue (APPLICATIONNS, "interval");
            if (newInterval != null)
                int.TryParse (newInterval, out interval);
        }

        private string GetAttributeStringValue (IAttributeSet attrs, string nameSpace, string name, string defaultValue)
        {
            var value = attrs.GetAttributeValue (nameSpace, name);

            if (value == null)
                value = defaultValue;

            return value;
        }


        protected override Android.Views.View OnCreateView (Android.Views.ViewGroup parent)
        {
            var view = base.OnCreateView (parent);

            // The basic preference layout puts the widget frame to the right of the title and summary,
            // so we need to change it a bit - the seekbar should be under them.
            var layout = (LinearLayout) view;
            layout.Orientation = Orientation.Vertical;

            return view;
        }

        protected override void OnBindView (Android.Views.View view)
        {
            base.OnBindView (view);

            try {
                // move our seekbar to the new view we've been given
                var oldContainer = seekBar.Parent;
                var newContainer = view.FindViewById<ViewGroup> (Resource.Id.seekBarPrefBarContainer);

                if (oldContainer != newContainer) {
                    // remove the seekbar from the old view
                    if (oldContainer != null) {
                        ((ViewGroup) oldContainer).RemoveView (seekBar);
                    }
                    // remove the existing seekbar (there may not be one) and add ours
                    newContainer.RemoveAllViews ();
                    newContainer.AddView (seekBar, ViewGroup.LayoutParams.FillParent,
                        ViewGroup.LayoutParams.WrapContent);
                }
            }
            catch(Exception ex) {
                Console.WriteLine ("Error Binding View: " + ex);
            }

            //if dependency is false from the beginning, disable the seek bar
            if (view != null && !view.Enabled)
            {
                seekBar.Enabled = false;
            }

            UpdateView (view);
        }

        protected void UpdateView (View view) 
        {

            try {
                statusText = view.FindViewById<TextView> (Resource.Id.seekBarPrefValue);

                statusText.Text = currentValue.ToString ();
                statusText.SetMinWidth (30);

                seekBar.Progress = currentValue - minValue;

                var unitsRightView = view.FindViewById<TextView> (Resource.Id.seekBarPrefUnitsRight);
                unitsRightView.Text = unitsRight;

                var unitsLeftView = view.FindViewById<TextView> (Resource.Id.seekBarPrefUnitsLeft);
                unitsLeftView.Text = unitsLeft;

            }
            catch(Exception e) {
                Console.WriteLine ("Error updating seek bar preference: " + e);
            }

        }

        public void OnProgressChanged (Android.Widget.SeekBar seekBar, int progress, bool fromUser)
        {
            int newValue = progress + minValue;

            if(newValue > maxValue)
                newValue = maxValue;
            else if(newValue < minValue)
                newValue = minValue;
            else if(interval != 1 && newValue % interval != 0)
                newValue = (int)Math.Round (((float)newValue)/interval)*interval;  
                
            // change rejected, revert to the previous value
            if(!CallChangeListener(newValue)){
                seekBar.Progress = currentValue - minValue; 
                return; 
            }

            // change accepted, store it
            currentValue = newValue;
            statusText.Text = newValue.ToString ();

            PersistInt (newValue);
        }

        public void OnStartTrackingTouch (Android.Widget.SeekBar seekBar)
        {
        }

        public void OnStopTrackingTouch (Android.Widget.SeekBar seekBar)
        {
            NotifyChanged ();
        }

        protected override Java.Lang.Object OnGetDefaultValue (Android.Content.Res.TypedArray ta, int index)
        {
            var defaultValue = ta.GetInt (index, DEFAULT_VALUE);
            return defaultValue;
        }

        protected override void OnSetInitialValue (bool restorePersistedValue, Java.Lang.Object defaultValue)
        {
            if (restorePersistedValue) {
                currentValue = GetPersistedInt (currentValue);
            } else {
                int temp = 0;
                try {
                    temp = ((Java.Lang.Integer)defaultValue).IntValue ();
                }
                catch {
                    Console.WriteLine ("Invalid default value: " + defaultValue.ToString());
                }

                PersistInt (temp);
                currentValue = temp;
            }
        }

        public override bool Enabled {
            get {
                return base.Enabled;
            }
            set {
                base.Enabled = value;

                seekBar.Enabled = value;
            }
        }

        public override void OnDependencyChanged (Preference dependency, bool disableDependent)
        {
            base.OnDependencyChanged (dependency, disableDependent);

            //Disable movement of seek bar when dependency is false
            if (seekBar != null) {
                seekBar.Enabled = !disableDependent;
            }
        }
    }
}

