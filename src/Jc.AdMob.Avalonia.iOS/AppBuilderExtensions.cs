using Avalonia;
using Google.MobileAds;

namespace Jc.AdMob.Avalonia.iOS;

public static class AppBuilderExtensions
{
    [Obsolete("Use AdMobOptions overload for consent service")]
    public static AppBuilder UseAdMob(this AppBuilder appBuilder, IReadOnlyCollection<string>? testDeviceIds = null)
    {
        MobileAds.SharedInstance.Start(null);

        return appBuilder.AfterSetup(_ =>
        {
            BannerAd.Implementation = new BannerAdiOS(testDeviceIds);
            Avalonia.InterstitialAd.ImplementationFactory = unitId => new InterstitialAd(unitId, testDeviceIds);
        });
    }

    public static AppBuilder UseAdMob(this AppBuilder appBuilder, AdMobOptions options)
    {
        return appBuilder.AfterSetup(_ =>
        {
            AdMob.Current.Consent = new AdConsentiOS(options);
            AdMob.Current.Interstitial = new Avalonia.InterstitialAd();

            Avalonia.InterstitialAd.ImplementationFactory = unitId => new InterstitialAd(options, unitId);
            BannerAd.Implementation = new BannerAdiOS(options);
            
            AdMob.Current.Initialize(options);
        });
    }
}