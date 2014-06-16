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

			if (Settings.Instance.HideLabels)
				label.Visibility = ViewStates.Gone;
				
			view.FindViewById<ImageView> (Resource.Id.imageIcon).SetImageDrawable (app.GetIcon (Context));

			return view;
		}

		public void Sort () 
		{
			Apps.Sort ((a1, a2) => Settings.Instance.GetAppOrder(a1.PackageName).CompareTo(Settings.Instance.GetAppOrder(a2.PackageName)));

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
