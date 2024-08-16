using Foundation;
using Avalonia;
using Avalonia.iOS;
using Avalonia.ReactiveUI;
using Jc.AdMob.Avalonia.iOS;

namespace Jc.AdMob.Avalonia.Sample.iOS;

// The UIApplicationDelegate for the application. This class is responsible for launching the 
// User Interface of the application, as well as listening (and optionally responding) to 
// application events from iOS.
[Register("AppDelegate")]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public partial class AppDelegate : AvaloniaAppDelegate<App>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI()
            .UseAdMob(new AdMobOptions
            {
                TestDeviceIds = ["00000000-0000-0000-0000-000000000000"],
                TagForUnderAgeOfConsent = false,
                TagForChildDirectedTreatment = false,
            });
    }
}