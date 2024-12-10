using Avalonia.iOS;
using Avalonia.Platform;
using Google.MobileAds;

namespace Jc.AdMob.Avalonia.iOS;

internal sealed class BannerAdiOS : INativeBannerAd
{
    private const string TestUnit = "ca-app-pub-3940256099942544/2435281174";

    private readonly AdMobOptions? _options;
    private readonly IReadOnlyCollection<string>? _testDeviceIds;

    public BannerAdiOS(IReadOnlyCollection<string>? testDeviceIds = null)
    {
        _testDeviceIds = testDeviceIds;
    }

    internal BannerAdiOS(AdMobOptions options)
    {
        _options = options;
    }

    public IPlatformHandle CreateControl(
        string? unitId,
        BannerAd wrapper,
        IPlatformHandle parent,
        Func<IPlatformHandle> createDefault)
    {
        var adSize = wrapper.AdSize switch
        {
            AdSize.Banner => GetSize(320, 50),
            AdSize.FullBanner => GetSize(486, 60),
            AdSize.LargeBanner => GetSize(320, 100),
            AdSize.Leaderboard => GetSize(728, 90),
            AdSize.MediumRectangle => GetSize(300, 250),
            AdSize.WideSkyscraper => GetSize(160, 600),
            //_ => AdSizeCons.GetCurrentOrientationAnchoredAdaptiveBannerAdSize(UIScreen.MainScreen.Bounds.Width)
            // TODO figure out some way to do this nicely or fix the binding
            _ => GetSize((int)UIScreen.MainScreen.Bounds.Width.Value, 100),
        };

        var adView = new BannerView()
        {
            AdSize = adSize,
            AdUnitId = string.IsNullOrWhiteSpace(unitId) ? TestUnit : unitId,
            RootViewController = GetRootViewController(),
        };

        if (_options is null || AdMob.Current.CanShowAds)
        {
            RenderBanner(adView, wrapper);
        }
        else
        {
            AdMob.Current.Consent.OnConsentProvided += (_, _) =>
            {
                RenderBanner(adView, wrapper);
            };
        }

        wrapper.Width = adSize.Size.Width;
        wrapper.Height = adSize.Size.Height;

        return new UIViewControlHandle(adView);
    }
    
    private void RenderBanner(BannerView adView, BannerAd wrapper)
    {
        if (_testDeviceIds is not null)
        {
            MobileAds.SharedInstance.RequestConfiguration.TestDeviceIdentifiers = _testDeviceIds.ToArray();
        }

        var request = _options is null ? Request.GetDefaultRequest() : AdRequest.GetRequest();

        adView.AdReceived += wrapper.AdLoaded;
        adView.ReceiveAdFailed += (s, e) => wrapper.AdFailedToLoad(s, new AdError(e.Error.DebugDescription));
        adView.ImpressionRecorded += wrapper.AdImpression;
        adView.ClickRecorded += wrapper.AdClicked;
        adView.WillPresentScreen += wrapper.AdOpened;
        adView.ScreenDismissed += wrapper.AdClosed;

        adView.LoadRequest(request);
    }

    private UIViewController GetRootViewController()
    {
        foreach (UIWindow window in UIApplication.SharedApplication.Windows)
        {
            if (window.RootViewController != null)
            {
                return window.RootViewController;
            }
        }

        return null;
    }

    private static Google.MobileAds.AdSize GetSize(int width, int height)
    {
        return new Google.MobileAds.AdSize
        {
            Size = new CGSize(width, height),
        };
    }
}