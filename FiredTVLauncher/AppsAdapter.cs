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
		public Activity Context { get; set; }

		List<AppInfo> apps = new List<AppInfo> ();

		public override long GetItemId (int position) { return position; } 
		public override int Count { get { return apps.Count; } }
		public override AppInfo this [int index] { get { return apps [index]; } }

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var app = apps [position];
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

		public void Reload () 
		{
			AppInfo.FetchApps (Context, false, r => {

				apps.Clear ();
				apps.AddRange (r);

				Context.RunOnUiThread (NotifyDataSetChanged);
			});
		}
	}
	
}
