using Android.Content;
using Android.Runtime;
using Android.Widget;
using Avalonia.Android;
using Avalonia.Controls.Platform;
using Avalonia.Platform;
using Avalonia.Threading;
using Google.Android.Libraries.Ads.Mobile.Sdk.Banner;
using Google.Android.Libraries.Ads.Mobile.Sdk.Common;
using SdkAdSize = Google.Android.Libraries.Ads.Mobile.Sdk.Banner.AdSize;

namespace Jc.AdMob.Avalonia.Android;

internal sealed class BannerAdAndroid : INativeBannerAd
{
    private const string AdaptiveTestUnit = "ca-app-pub-3940256099942544/9214589741";
    private const string FixedTestUnit = "ca-app-pub-3940256099942544/6300978111";

    public IPlatformHandle? CreateControl(
        string? unitId, BannerAd wrapper, IPlatformHandle parent, Func<IPlatformHandle> createDefault)
    {
        try
        {
            if (wrapper.AdSize is AdSize.Invalid)
            {
                throw new InvalidOperationException("Invalid banner ad size.");
            }

            var context = (parent as AndroidViewControlHandle)?.View.Context ?? Application.Context!;
            var fixedSize = wrapper.AdSize switch
            {
                AdSize.Banner => SdkAdSize.Banner,
                AdSize.FullBanner => SdkAdSize.FullBanner,
                AdSize.LargeBanner => SdkAdSize.LargeBanner,
                AdSize.Leaderboard => SdkAdSize.Leaderboard,
                AdSize.MediumRectangle => SdkAdSize.MediumRectangle,
                // Next-Gen exposes no predefined wide skyscraper size.
                AdSize.WideSkyscraper => new SdkAdSize(160, 600),
                _ => null
            };
            var adaptive = fixedSize is null;
            var size = fixedSize ?? GetAdaptiveSize(context);
            var adUnit = string.IsNullOrWhiteSpace(unitId)
                ? (adaptive ? AdaptiveTestUnit : FixedTestUnit)
                : unitId;

            var density = context.Resources?.DisplayMetrics?.Density ?? 1;
            SizeBanner(wrapper, size, density);

            var adView = new AdView(context);
            // Avalonia disposes the hosted view before it calls the handle's cleanup.
            var host = new FrameLayout(context);
            host.AddView(adView, new FrameLayout.LayoutParams(-1, -1));
            var handle = new BannerHandle(host, adView, wrapper, adUnit, size, adaptive, density,
                (AdConsentAndroid)AdMob.Current.Consent);
            Dispatcher.UIThread.Post(handle.WaitForAds);
            return handle;
        }
        catch (Exception error)
        {
            Dispatcher.UIThread.Post(() => wrapper.AdFailedToLoad(this, new AdError(error.Message)));
            return null;
        }
    }

    private static void SizeBanner(BannerAd wrapper, SdkAdSize size, double density)
    {
        // Avalonia truncates the transformed bounds to device pixels. Leave a fraction
        // of a pixel so the coordinate transform cannot round an edge down.
        wrapper.Width = (Math.Ceiling(size.Width * density) + 0.25) / density;
        wrapper.Height = (Math.Ceiling(size.Height * density) + 0.25) / density;
    }

    private static SdkAdSize GetAdaptiveSize(Context context)
    {
        var metrics = context.Resources!.DisplayMetrics!;
        // Keep the existing anchored adaptive size instead of changing the banner height.
#pragma warning disable CS0618
        return SdkAdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSize(
            context, (int)(metrics.WidthPixels / metrics.Density));
#pragma warning restore CS0618
    }

    private sealed class BannerHandle(
        FrameLayout host, AdView view, BannerAd wrapper, string unitId, SdkAdSize size,
        bool adaptive, double density, AdConsentAndroid consent)
        : AndroidViewControlHandle(host), INativeControlHostDestroyableControlHandle
    {
        private readonly BannerAd _wrapper = wrapper;
        private bool _disposed;
        private bool _initializationFailed;
        private bool _personalized;
        private BannerCallback? _callback;

        internal void WaitForAds()
        {
            if (_disposed)
            {
                return;
            }

            consent.EligibilityChanged += TryLoad;
            consent.OnConsentFailedToInitialize += InitializationFailed;
            // Adding this handler can synchronously replay readiness.
            AdMob.Current.OnAdsInitialized += ReadinessChanged;
            TryLoad();
        }

        private void ReadinessChanged(object? sender, EventArgs args) => TryLoad();

        private void InitializationFailed(object? sender, AdError error)
        {
            if (consent.SdkInitialization.IsCompleted)
            {
                TryLoad();
            }
        }

        private void TryLoad()
        {
            if (_disposed || _initializationFailed || !consent.SdkInitialization.IsCompletedSuccessfully)
            {
                return;
            }

            if (consent.SdkInitialization.Result is { } error)
            {
                _initializationFailed = true;
                Unsubscribe();
                _wrapper.AdFailedToLoad(this, error);
                return;
            }

            var canShowAds = AdMob.Current.CanShowAds;
            var personalized = consent.CanShowPersonalizedAds;
            if (_callback is not null)
            {
                if (canShowAds && personalized == _personalized)
                {
                    return;
                }

                _callback = null;
                // Cancel the old request and its automatic refresh before using new consent.
                view.UnregisterBannerAd()?.Destroy();
            }

            if (!canShowAds)
            {
                return;
            }

            try
            {
                _personalized = personalized;
                _callback = new BannerCallback(this);
                using var builder = new BannerAdRequest.Builder(unitId, size);
                AdRequest.ApplyConsent(builder);
                view.LoadAd(builder.Build(), _callback);
            }
            catch (Exception exception)
            {
                _callback = null;
                _wrapper.AdFailedToLoad(this, new AdError(exception.Message));
            }
        }

        private void Unsubscribe()
        {
            consent.EligibilityChanged -= TryLoad;
            consent.OnConsentFailedToInitialize -= InitializationFailed;
            AdMob.Current.OnAdsInitialized -= ReadinessChanged;
        }

        void INativeControlHostDestroyableControlHandle.Destroy()
        {
            _disposed = true;
            _callback = null;
            Unsubscribe();
            view.Destroy();
            view.Dispose();
            base.Destroy();
        }

        private sealed class BannerCallback(BannerHandle owner)
            : Java.Lang.Object, IAdLoadCallback, IBannerAdEventCallback, IBannerAdRefreshCallback
        {
            public void OnAdLoaded(Java.Lang.Object? ad) => Dispatch(() =>
            {
                if (ad?.JavaCast<IBannerAd>() is not { } banner)
                {
                    owner._wrapper.AdFailedToLoad(this, new AdError("The ad SDK returned no banner."));
                    return;
                }

                banner.AdEventCallback = this;
                banner.BannerAdRefreshCallback = this;
                owner.BannerLoaded(banner.AdSize, this);
            });

            public void OnAdRefreshed() => Dispatch(() => owner.BannerRefreshed(this));
            public void OnAdFailedToLoad(LoadAdError error) => OnAdFailedToRefresh(error);

            public void OnAdFailedToRefresh(LoadAdError error)
            {
                var message = error.Message;
                Dispatch(() => owner._wrapper.AdFailedToLoad(this, new AdError(message)));
            }

            public void OnAdImpression() => Dispatch(() => owner._wrapper.AdImpression(this, EventArgs.Empty));
            public void OnAdClicked() => Dispatch(() => owner._wrapper.AdClicked(this, EventArgs.Empty));
            public void OnAdShowedFullScreenContent() => Dispatch(() => owner._wrapper.AdOpened(this, EventArgs.Empty));
            public void OnAdDismissedFullScreenContent() => Dispatch(() => owner._wrapper.AdClosed(this, EventArgs.Empty));

            private void Dispatch(Action action) => Dispatcher.UIThread.Post(() =>
            {
                if (ReferenceEquals(owner._callback, this))
                {
                    action();
                }
            });
        }

        private void BannerRefreshed(object sender) => BannerLoaded(view.BannerAd?.AdSize, sender);

        private void BannerLoaded(SdkAdSize? loadedSize, object sender)
        {
            if (adaptive && loadedSize is not null)
            {
                SizeBanner(_wrapper, loadedSize, density);
            }

            _wrapper.AdLoaded(sender, EventArgs.Empty);
        }
    }
}
