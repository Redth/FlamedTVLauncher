# FiredTV Launcher

FiredTV Launcher is a 'replacement' for Amazon's Home/Launcher app which does not show sideloaded apps.

![FiredTV Launcher](Art/FiredTVLauncher-Pic.png)

###How does it work?
While it's not possible (yet/without root) to actually replace the launcher app, FiredTV effectively does this by watching for Amazon's launcher being opened, and then immediately launches itself.  Because of this, you will see Amazon's home screen flash on the screen very briefly after pressing the home button, before FiredTV Launcher appears.

###Where's the Java?
If you looked at the source, you may have noticed that there's no Java.  That's because this was written entirely in C# using [Xamarin](http://xamarin.com)!  C# is awesome, go use Xamarin!