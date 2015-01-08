
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
using Launchers.Common;

namespace FiredTVLauncher
{
	[Activity (Label = "FiredTV Settings - Hide Apps")]			
	public class SettingsAppShowHideActivity : ListActivity
	{
		SettingsAppShowAdapter adapter;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here
			adapter = new SettingsAppShowAdapter { Context = this };
			ListView.Adapter = adapter;

			ListView.ItemClick += (sender, e) => {

				var app = adapter[e.Position];
				var cb = e.View.FindViewById<CheckBox>(Resource.Id.checkBox);

				cb.Checked = !cb.Checked;

				if (!cb.Checked)
					Settings.Instance.Blacklist.RemoveAll (s => s == app.PackageName);
				else 
					Settings.Instance.Blacklist.Add (app.PackageName);

				Settings.Save ();

			};
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			adapter.Reload ();
		}
	}

	class SettingsAppShowAdapter : BaseAdapter <AppInfo>
	{
		List<AppInfo> apps = new List<AppInfo> ();

		public Activity Context { get; set; }
		public override long GetItemId (int position) { return position; }
		public override int Count { get { return apps.Count; } }
		public override AppInfo this [int index] { get { return apps [index]; } }

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var app = apps [position];

			var view = convertView ??
			           LayoutInflater.FromContext (this.Context).Inflate (Resource.Layout.AppShowHideItem, parent, false);

			view.FindViewById<TextView> (Resource.Id.textView).Text = app.Name;
			var icon = app.GetIcon (Context);
			if (icon != null)
				view.FindViewById<ImageView> (Resource.Id.imageView).SetImageDrawable (icon);

			view.FindViewById<CheckBox> (Resource.Id.checkBox).Checked = Settings.Instance.Blacklist.Contains (app.PackageName);

			return view;
		}

		public void Reload ()
		{
            AppInfo.FetchApps (Context, null, true, r => {

				apps.Clear ();
				apps.AddRange (r);

				Context.RunOnUiThread (NotifyDataSetChanged);
			});
		}
	}
}

