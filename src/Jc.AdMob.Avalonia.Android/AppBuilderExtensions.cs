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
            Avalonia.RewardedInterstitialAd.ImplementationFactory = (unitId) => new RewardedInterstitialAd(unitId, testDeviceIds);
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
            RewardedInterstitialAd.Activity = activity;
            Avalonia.RewardedInterstitialAd.ImplementationFactory = (unitId) => new RewardedInterstitialAd(unitId, testDeviceIds);
            RewardedAd.Activity = activity;
            Avalonia.RewardedAd.ImplementationFactory = (unitId) => new RewardedAd(unitId, testDeviceIds);
        });
    }

    public static AppBuilder UseAdMob(this AppBuilder appBuilder, Activity activity, AdMobOptions options)
    {
        return appBuilder.AfterSetup(_ =>
        {
            AdMob.Current.Consent = new AdConsentAndroid(activity, options);
            AdMob.Current.Interstitial = new Avalonia.InterstitialAd();
            AdMob.Current.RewardedInterstitial = new Avalonia.RewardedInterstitialAd();
            AdMob.Current.Rewarded = new Avalonia.RewardedAd();
            
            InterstitialAd.Activity = activity;
            Avalonia.InterstitialAd.ImplementationFactory = (unitId) => new InterstitialAd(options, unitId);
            RewardedInterstitialAd.Activity = activity;
            Avalonia.RewardedInterstitialAd.ImplementationFactory = (unitId) => new RewardedInterstitialAd(options, unitId);
            RewardedAd.Activity = activity;
            Avalonia.RewardedAd.ImplementationFactory = (unitId) => new RewardedAd(options, unitId);
            
            BannerAd.Implementation = new BannerAdAndroid(options);
            
            AdMob.Current.Initialize(options);
        });
    }
}