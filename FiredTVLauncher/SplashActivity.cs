using Android.App;
using Android.OS;

namespace FiredTVLauncher
{
    [Activity(Theme = "@style/Theme.Splash", MainLauncher = true, NoHistory = true, Label = "FiredTV")]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            StartActivity(typeof(MainActivity));
        }
    }
}

