using System;

namespace FiredTVLauncher
{
    public class Log
    {
        public Log ()
        {
        }

        const string TAG = "FIRED-TV";

        public static void Debug (string message, params string[] objs)
        {
            Android.Util.Log.Debug (TAG, string.Format (message, objs));
        }

        public static void Error (string message, Exception ex)
        {
            Android.Util.Log.Error (TAG, message + " -> " + ex);
        }
    }
}

