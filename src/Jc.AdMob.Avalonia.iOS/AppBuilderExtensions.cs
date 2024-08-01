using Avalonia;
using Google.MobileAds;

namespace Jc.AdMob.Avalonia.iOS;

public static class AppBuilderExtensions
{
    public static AppBuilder UseAdMob(this AppBuilder appBuilder, IReadOnlyCollection<string>? testDeviceIds = null)
    {
        MobileAds.SharedInstance.Start(null);

        return appBuilder.AfterSetup(_ =>
        {
            BannerAd.Implementation = new BannerAdiOS(testDeviceIds);
            Avalonia.InterstitialAd.ImplementationFactory = (unitId) => new InterstitialAd(unitId, testDeviceIds);
        });
    }
}