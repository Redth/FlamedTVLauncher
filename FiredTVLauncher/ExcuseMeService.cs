using System;
using Android.App;
using System.IO;
using System.Threading.Tasks;
using Android.Widget;
using Android.Content;
using System.Threading;
using System.Linq;

namespace FiredTVLauncher
{
	[Service]
	public class ExcuseMeService : Service
	{
		public ExcuseMeService ()
		{
		}

        Timer timer;

        DateTime acted = DateTime.MinValue;

        public static bool AllowFireTVHome = false;
        ActivityManager activityManager;

        public override void OnCreate ()
        {
            Console.WriteLine ("ExcuseMe Service Created");
        }

        public override void OnDestroy ()
        {
            Console.WriteLine ("ExcuseMe Service Destroyed");
        }

        public override Android.OS.IBinder OnBind (Intent intent)
        {
            return default(Android.OS.IBinder);
        }

        public override StartCommandResult OnStartCommand (Intent intent, StartCommandFlags flags, int startId)
        {
            if (!Settings.IsFireTV ()) {
                Console.WriteLine ("Not starting KFTV Watcher Service, not FireTV...");
                return StartCommandResult.NotSticky;
            }

            if (timer == null) {
                activityManager = ActivityManager.FromContext (this);

                timer = new Timer (state => {

                    if (Settings.Instance.DisableHomeDetection) {
                        Console.WriteLine ("Disabled Home Detection... Not starting KFTV Watcher Service...");
                        return;
                    }
                     
                    // Gets the topmost running task
                    var topTask = activityManager.GetRunningTasks (1).FirstOrDefault ();

                    if (topTask == null || topTask.TopActivity == null)
                        return;

                    // We can detect that the firetv home launcher was called
                    // by simply getting the top task's top activity and matching the 
                    // package name and class name
                    //      Package Name: com.amazon.tv.launcher
                    //      Class Name:   com.amazon.tv.launcher.ui.HomeActivity
                    if (topTask.TopActivity.PackageName == Settings.HOME_PACKAGE_NAME
                        && topTask.TopActivity.ClassName == Settings.HOME_CLASS_NAME) {

                        var actedAgo = DateTime.UtcNow - acted;
                        // Just to be safe, we don't want to call this multiple times in a row
                        // Also there's a static flag that the firedtv app can set
                        // to allow the user to launch the firetv homescreen
                        if (actedAgo > TimeSpan.FromMilliseconds (500) && !AllowFireTVHome) {

                            //Come back to papa
                            var actIntent = new Intent(ApplicationContext, typeof(MainActivity));
                            actIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.SingleTop);
                            StartActivity (actIntent);

                            // Set the flag
                            acted = DateTime.UtcNow;
                        }

                    } else {
                        // It wasn't a match (another task is top) so reset the flag
                        acted = DateTime.MinValue;
                    }

                }, null, Settings.Instance.HomeDetectIntervalMs, Settings.Instance.HomeDetectIntervalMs);
            }
        
            return StartCommandResult.Sticky;
        }
	}
}


