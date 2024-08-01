using Android.Content;
using Android.Gms.Ads;
using Android.Util;
using Avalonia.Android;
using Avalonia.Platform;

namespace Jc.AdMob.Avalonia.Android;

internal sealed class BannerAdAndroid : INativeBannerAd
{
    private const string TestUnit = "ca-app-pub-3940256099942544/9214589741";

    private readonly IReadOnlyCollection<string>? _testDeviceIds;

    public BannerAdAndroid(IReadOnlyCollection<string>? testDeviceIds)
    {
        _testDeviceIds = testDeviceIds;
    }
    
    public IPlatformHandle CreateControl(
        string? unitId, BannerAd wrapper,
        IPlatformHandle parent,
        Func<IPlatformHandle> createDefault)
    {
        var parentContext = (parent as AndroidViewControlHandle)?.View.Context ??
                            Application.Context;

        var adSize = wrapper.AdSize switch
        {
            AdSize.Banner => global::Android.Gms.Ads.AdSize.Banner,
            AdSize.FullBanner => global::Android.Gms.Ads.AdSize.FullBanner,
            AdSize.Invalid => global::Android.Gms.Ads.AdSize.Invalid,
            AdSize.LargeBanner => global::Android.Gms.Ads.AdSize.LargeBanner,
            AdSize.Leaderboard => global::Android.Gms.Ads.AdSize.Leaderboard,
            AdSize.MediumRectangle => global::Android.Gms.Ads.AdSize.MediumRectangle,
            AdSize.WideSkyscraper => global::Android.Gms.Ads.AdSize.WideSkyscraper,
            _ => global::Android.Gms.Ads.AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSize(parentContext,
                GetScreenWidth(parentContext))
        };

        var adView = new AdView(parentContext)
        {
            AdSize = adSize,
            AdUnitId = string.IsNullOrWhiteSpace(unitId) ? TestUnit : unitId,
        };

        var configBuilder = new RequestConfiguration.Builder();
        if (_testDeviceIds is not null)
        {
            configBuilder.SetTestDeviceIds(_testDeviceIds.ToList());
        }
        MobileAds.RequestConfiguration = configBuilder.Build();

        var requestBuilder = new AdRequest.Builder();
        var adRequest = requestBuilder.Build();

        var listener = new BannerAdListener();
        listener.AdLoaded += wrapper.AdLoaded;
        listener.AdFailedToLoad += (s, e) => wrapper.AdFailedToLoad(s, new AdError(e.Message));
        listener.AdImpression += wrapper.AdImpression;
        listener.AdClicked += wrapper.AdClicked;
        listener.AdSwiped += wrapper.AdSwiped;
        listener.AdOpened += wrapper.AdOpened;
        listener.AdClosed += wrapper.AdClosed;

        adView.AdListener = listener;
        adView.LoadAd(adRequest);

        
        wrapper.Width = adSize.Width;
        wrapper.Height = adSize.Height;

        return new AndroidViewControlHandle(adView);
    }
    
    [Obsolete("This call site is reachable on: 'Android' 21.0 and later. 'Display.GetMetrics(DisplayMetrics?)' is obsoleted on: 'Android' 30.0 and later.")]
    int GetScreenWidth(Context context)
    {
        var displayMetrics = new DisplayMetrics();
        context.Display.GetMetrics(displayMetrics);

        return (int)(displayMetrics.WidthPixels / displayMetrics.Density);
    }
}