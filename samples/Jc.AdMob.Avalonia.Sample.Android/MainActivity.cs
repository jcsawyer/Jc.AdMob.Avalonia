using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Jc.AdMob.Avalonia.Android;
using ReactiveUI.Avalonia;

namespace Jc.AdMob.Avalonia.Sample.Android;

[Application]
public class AndroidApp : AvaloniaAndroidApplication<App>
{
    protected AndroidApp(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }
    
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI(_ => { })
            .UseAdMob(new AdMobOptions
            {
                TestDeviceIds = ["00000000-0000-0000-0000-000000000000"],
                TagForUnderAgeOfConsent = false,
                TagForChildDirectedTreatment = false,
                AppId = "ca-app-pub-9127086931863581~2923947432",
            });
    }
}

[Activity(
    Label = "Jc.AdMob.Avalonia.Sample.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    
}
