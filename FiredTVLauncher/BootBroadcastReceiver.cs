
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
	[BroadcastReceiver]
	[IntentFilter(new String[]{ Intent.ActionBootCompleted }, Priority = (int)IntentFilterPriority.LowPriority)]
	public class BootBroadcastReceiver : BroadcastReceiver
	{
		public override void OnReceive (Context context, Intent intent)
		{
			context.StartService (new Intent(context, typeof(ExcuseMeService)));
		}
	}

    [BroadcastReceiver]
    [IntentFilter(new String[]{ Intent.ActionScreenOn })]
    public class ScreenOnBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive (Context context, Intent intent)
        {
            context.StartService (new Intent(context, typeof(ExcuseMeService)));
        }
    }
}

