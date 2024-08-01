using Avalonia;

namespace Jc.AdMob.Avalonia.Android;

public static class AppBuilderExtensions
{
    public static AppBuilder UseAdMob(this AppBuilder appBuilder, IReadOnlyCollection<string>? testDeviceIds = null)
    {
        return appBuilder.AfterSetup(_ =>
        {
            BannerAd.Implementation = new BannerAdAndroid(testDeviceIds);
            Avalonia.InterstitialAd.ImplementationFactory = (unitId) => new InterstitialAd(unitId, testDeviceIds);
        });
    }

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
}