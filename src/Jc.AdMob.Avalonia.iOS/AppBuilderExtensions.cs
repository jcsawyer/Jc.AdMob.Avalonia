using Avalonia;
using Google.MobileAds;

namespace Jc.AdMob.Avalonia.iOS;

public static class AppBuilderExtensions
{
    public static AppBuilder UseAdMob(this AppBuilder appBuilder)
    {
        MobileAds.SharedInstance.Start(null);

        return appBuilder.AfterSetup(_ =>
        {
            BannerAd.Implementation = new BannerAdiOS();
        });
    }
}