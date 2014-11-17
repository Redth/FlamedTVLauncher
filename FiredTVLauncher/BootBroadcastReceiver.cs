using Android.App;
using Android.Content;

namespace FiredTVLauncher
{
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionBootCompleted }, Priority = (int)IntentFilterPriority.LowPriority)]
    public class BootBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            context.StartService(new Intent(context, typeof(ExcuseMeService)));
        }
    }
}

