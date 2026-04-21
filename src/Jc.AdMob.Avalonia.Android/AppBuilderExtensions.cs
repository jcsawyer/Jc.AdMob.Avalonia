using Avalonia;

namespace Jc.AdMob.Avalonia.Android;

public static class AppBuilderExtensions
{
    public static AppBuilder UseAdMob(this AppBuilder appBuilder, AdMobOptions options)
    {
        return appBuilder.AfterSetup(_ =>
        {
            if (global::Android.App.Application.Context is not global::Android.App.Application application)
            {
                throw new InvalidOperationException("Android application context is unavailable.");
            }

            ActivityProvider.Initialize(application);

            AdMob.Current.AppOpen = new Avalonia.AppOpenAd();
            AdMob.Current.Consent = new AdConsentAndroid(options);
            AdMob.Current.Interstitial = new Avalonia.InterstitialAd();
            AdMob.Current.RewardedInterstitial = new Avalonia.RewardedInterstitialAd();
            AdMob.Current.Rewarded = new Avalonia.RewardedAd();
            
            Avalonia.AppOpenAd.ImplementationFactory = (unitId) => new AppOpenAd(options, unitId);
            Avalonia.InterstitialAd.ImplementationFactory = (unitId) => new InterstitialAd(options, unitId);
            Avalonia.RewardedInterstitialAd.ImplementationFactory = (unitId) => new RewardedInterstitialAd(options, unitId);
            Avalonia.RewardedAd.ImplementationFactory = (unitId) => new RewardedAd(options, unitId);
            
            BannerAd.Implementation = new BannerAdAndroid(options);
            
            AdMob.Current.Initialize(options);
        });
    }
}
