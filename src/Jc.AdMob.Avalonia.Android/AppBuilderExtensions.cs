using Avalonia;

namespace Jc.AdMob.Avalonia.Android;

public static class AppBuilderExtensions
{
    public static AppBuilder UseAdMob(this AppBuilder appBuilder, Activity activity, AdMobOptions options)
    {
        return appBuilder.AfterSetup(_ =>
        {
            AdMob.Current.AppOpen = new Avalonia.AppOpenAd();
            AdMob.Current.Consent = new AdConsentAndroid(activity, options);
            AdMob.Current.Interstitial = new Avalonia.InterstitialAd();
            AdMob.Current.RewardedInterstitial = new Avalonia.RewardedInterstitialAd();
            AdMob.Current.Rewarded = new Avalonia.RewardedAd();
            
            AppOpenAd.Activity = activity;
            Avalonia.AppOpenAd.ImplementationFactory = (unitId) => new AppOpenAd(options, unitId);
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