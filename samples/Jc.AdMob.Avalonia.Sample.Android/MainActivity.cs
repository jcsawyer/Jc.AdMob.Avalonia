using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;
using Jc.AdMob.Avalonia.Android;

namespace Jc.AdMob.Avalonia.Sample.Android;

[Activity(
    Label = "Jc.AdMob.Avalonia.Sample.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI()
            .UseAdMob(this, new AdMobOptions
            {
                TestDeviceIds = ["00000000-0000-0000-0000-000000000000"],
                TagForUnderAgeOfConsent = false,
                TagForChildDirectedTreatment = false,
                AppId = "ca-app-pub-9127086931863581~2923947432",
            });
    }
}