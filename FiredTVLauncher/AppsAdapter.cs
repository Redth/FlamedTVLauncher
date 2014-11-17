using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Content.PM;
using Android.Content.Res;

namespace FiredTVLauncher
{

	public class AppsAdapter : BaseAdapter<AppInfo>
	{
		public AppsAdapter ()
		{
			Apps = new List<AppInfo> ();
		}

		public Activity Context { get; set; }

		public List<AppInfo> Apps { get; set; }

		public override long GetItemId (int position) { return position; } 
		public override int Count { get { return Apps.Count; } }
		public override AppInfo this [int index] { get { return Apps [index]; } }

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var app = Apps [position];
			var view = convertView ??
				LayoutInflater.FromContext (Context).Inflate (Resource.Layout.GridItemLayout, parent, false);

			var label = view.FindViewById<TextView> (Resource.Id.textName);
			label.Text = app.Name;
			label.TextSize = Settings.Instance.LabelFontSize;

            label.Visibility = Settings.Instance.HideLabels ? ViewStates.Gone : ViewStates.Visible;
				
            var imgView = view.FindViewById<ImageView> (Resource.Id.imageIcon);
                
            imgView.SetImageDrawable (app.GetIcon (Context));

            var iconBg = view.FindViewById<LinearLayout> (Resource.Id.iconBackground);

            iconBg.SetBackgroundColor (new Android.Graphics.Color (0, 0, 0, Settings.Instance.IconBackgroundAlpha));
            label.SetBackgroundColor (new Android.Graphics.Color (0, 0, 0, Settings.Instance.LabelBackgroundAlpha));
			return view;
		}

		public void Sort () 
		{
			Apps.Sort ((a1, a2) => Settings.Instance.GetAppOrder(a1.PackageName).Order.CompareTo(Settings.Instance.GetAppOrder(a2.PackageName).Order));

		}


		public void Reload () 
		{
			AppInfo.FetchApps (Context, false, r => {

				Apps.Clear ();
				Apps.AddRange (r);

				Settings.Instance.SanitizeAppOrder (Apps);

				Sort ();

				Context.RunOnUiThread (NotifyDataSetChanged);
			});
		}
	}
	
}
