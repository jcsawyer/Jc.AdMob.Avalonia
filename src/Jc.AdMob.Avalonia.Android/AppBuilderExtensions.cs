using Avalonia;

namespace Jc.AdMob.Avalonia.Android;

public static class AppBuilderExtensions
{
    [Obsolete("Use AdMobOptions overload with activity for interstitial ads and consent service")]
    public static AppBuilder UseAdMob(this AppBuilder appBuilder, IReadOnlyCollection<string>? testDeviceIds = null)
    {
        return appBuilder.AfterSetup(_ =>
        {
            BannerAd.Implementation = new BannerAdAndroid(testDeviceIds);
            Avalonia.InterstitialAd.ImplementationFactory = (unitId) => new InterstitialAd(unitId, testDeviceIds);
        });
    }

    [Obsolete("Use AdMobOptions overload for consent service")]
    public static AppBuilder UseAdMob(this AppBuilder appBuilder, Activity activity,
        IReadOnlyCollection<string>? testDeviceIds = null)
    {
        return appBuilder.AfterSetup(_ =>
        {
            BannerAd.Implementation = new BannerAdAndroid(testDeviceIds);
            InterstitialAd.Activity = activity;
            Avalonia.InterstitialAd.ImplementationFactory = (unitId) => new InterstitialAd(unitId, testDeviceIds);
        });
    }

    public static AppBuilder UseAdMob(this AppBuilder appBuilder, Activity activity, AdMobOptions options)
    {
        return appBuilder.AfterSetup(_ =>
        {
            AdMob.Current.Consent = new AdConsentAndroid(activity, options);
            AdMob.Current.Interstitial = new Avalonia.InterstitialAd();
            
            InterstitialAd.Activity = activity;
            Avalonia.InterstitialAd.ImplementationFactory = (unitId) => new InterstitialAd(options, unitId);
            
            BannerAd.Implementation = new BannerAdAndroid(options);
            
            AdMob.Current.Initialize(options);
        });
    }
}