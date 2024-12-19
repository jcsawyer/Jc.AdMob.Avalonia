using Avalonia;
using Google.MobileAds;

namespace Jc.AdMob.Avalonia.iOS;

public static class AppBuilderExtensions
{
    public static AppBuilder UseAdMob(this AppBuilder appBuilder, AdMobOptions options)
    {
        return appBuilder.AfterSetup(_ =>
        {
            AdMob.Current.Consent = new AdConsentiOS(options);
            AdMob.Current.Interstitial = new Avalonia.InterstitialAd();
            AdMob.Current.RewardedInterstitial = new Avalonia.RewardedInterstitialAd();
            AdMob.Current.Rewarded = new Avalonia.RewardedAd();

            Avalonia.InterstitialAd.ImplementationFactory = unitId => new InterstitialAd(options, unitId);
            Avalonia.RewardedInterstitialAd.ImplementationFactory = unitId => new RewardedInterstitialAd(options, unitId);
            Avalonia.RewardedAd.ImplementationFactory = unitId => new RewardedAd(options, unitId);
            BannerAd.Implementation = new BannerAdiOS(options);
            
            AdMob.Current.Initialize(options);
        });
    }
}