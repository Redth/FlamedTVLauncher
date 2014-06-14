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
	public class ExcuseMeService : IntentService
	{
		const string HOME_PACKAGE_NAME = "com.amazon.tv.launcher";
		const string HOME_CLASS_NAME = "com.amazon.tv.launcher.ui.HomeActivity";
		const int CHECK_INTERVAL_MS = 700;

		public ExcuseMeService ()
		{
		}

		Timer timer;
		bool didAct = false;

		protected override void OnHandleIntent (Intent intent)
		{
			if (timer == null) {
				timer = new Timer (state => {

					DateTime started = DateTime.UtcNow;

					var am = ActivityManager.FromContext (this);
					var topTask = am.GetRunningTasks (1).FirstOrDefault ();

					if (topTask == null || topTask.TopActivity == null)
						return;

					// We can detect that the firetv home launcher was called
					// by simply getting the top task's top activity and matching the 
					// package name and class name
					//		Package Name: com.amazon.tv.launcher
					//		Class Name:   com.amazon.tv.launcher.ui.HomeActivity
					if (topTask.TopActivity.PackageName == HOME_PACKAGE_NAME
						&& topTask.TopActivity.ClassName == HOME_CLASS_NAME) {

						// Just to be safe, we don't want to call this multiple times in a row
						if (!didAct) {

							//Come back to papa
							var actIntent = new Intent(ApplicationContext, typeof(MainActivity));
							actIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.SingleTop);
							StartActivity (actIntent);

							// Set the flag
							didAct = true;
						}

					} else {
						// It wasn't a match (another task is top) so reset the flag
						didAct = false;
					}

					var finished = DateTime.UtcNow - started;
					Console.WriteLine ("Took: " + finished.TotalMilliseconds + " ms");

				}, null, CHECK_INTERVAL_MS, CHECK_INTERVAL_MS);
			}
		}
	}
}


