using Avalonia;

namespace Jc.AdMob.Avalonia.Android;

public static class AppBuilderExtensions
{
    public static AppBuilder UseAdMob(this AppBuilder appBuilder)
    {
        return appBuilder.AfterSetup(_ =>
        {
            BannerAd.Implementation = new BannerAdAndroid();
        });
    }
}