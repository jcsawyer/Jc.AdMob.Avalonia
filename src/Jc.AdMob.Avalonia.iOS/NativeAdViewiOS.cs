using Avalonia;
using Avalonia.iOS;
using Avalonia.Platform;
using Google.MobileAds;
using SdkView = Google.MobileAds.NativeAdView;

namespace Jc.AdMob.Avalonia.iOS;

internal sealed class NativeAdViewiOS : NativeAdPlatformView
{
    private const string TestUnit = "ca-app-pub-3940256099942544/3986624511";

    private readonly AdHost _host = new() { ClipsToBounds = true };
    private readonly UIViewControlHandle _handle;
    private readonly UIView _parent;
    private readonly SdkView _adView = new(CGRect.Empty) { ClipsToBounds = true };
    private readonly UIView _clip = new() { ClipsToBounds = true };
    private readonly UIView[] _views;
    private AdLoader? _loader;
    private LoaderDelegate? _loaderDelegate;
    private AdDelegate? _adDelegate;
    private NativeAd? _ad;
    private double _aspectRatio;
    private Rect _viewport;
    private IReadOnlyList<Rect> _assetBounds = [];

    public NativeAdViewiOS(IPlatformHandle parent, IReadOnlyList<NativeAdAsset> assets)
    {
        _parent = ((UIViewControlHandle)parent).View;
        _handle = new UIViewControlHandle(_host);
        _host.AddSubview(_clip);
        _clip.AddSubview(_adView);
        _views = assets.Select(asset =>
        {
            var view = CreateAsset(asset);
            _adView.AddSubview(view);
            return view;
        }).ToArray();
        _host.ArrangeAd = ApplyFrames;
    }

    public override IPlatformHandle Handle => _handle;

    public override void Load(string? unitId)
    {
        UIResponder? responder = _parent;
        while (responder is not null && responder is not UIViewController)
            responder = responder.NextResponder;

        var controller = responder as UIViewController ?? _parent.Window?.RootViewController;
        _loaderDelegate = new LoaderDelegate(this);
        _loader = new AdLoader(string.IsNullOrWhiteSpace(unitId) ? TestUnit : unitId,
            controller, [AdLoadAdTypeConstants.Native], null)
        {
            Delegate = _loaderDelegate
        };
        _loader.LoadRequest(AdRequest.GetRequest());
    }

    public override void RegisterAd()
    {
        // Giving the SDK media while its view is zero-sized lets it lay out
        // creative-specific subviews before the ad has its final geometry.
        var ad = _ad!;
        ApplyFrames();
        _adView.MediaView.MediaContent = ad.MediaContent;
        _host.LayoutIfNeeded();
        _adView.NativeAd = ad;
    }

    public override bool HasAsset(int index) => !_views[index].Hidden;

    public override Size Measure(int index, Size available)
    {
        var view = _views[index];
        if (view is MediaView)
        {
            return MediaSize(available, _aspectRatio);
        }

        if (view is UIImageView)
        {
            return new Size(40, 40);
        }

        var size = view.SizeThatFits(new CGSize(
            double.IsFinite(available.Width) ? available.Width : 100_000,
            double.IsFinite(available.Height) ? available.Height : 100_000));

        return view.AccessibilityIdentifier switch
        {
            "ad-attribution" => new Size(Math.Max(15, size.Width), Math.Max(15, size.Height)),
            "ad-call-to-action" => new Size(size.Width + 16, size.Height + 12),
            _ => new Size(size.Width, size.Height)
        };
    }

    public override void Arrange(Size size, Rect viewport, IReadOnlyList<Rect> assetBounds)
    {
        _viewport = viewport;
        _assetBounds = assetBounds;
        ApplyFrames();
    }

    private void ApplyFrames()
    {
        if (_assetBounds.Count == 0)
        {
            return;
        }

        // Avalonia rounds the hosted UIView in window coordinates. Use the resulting
        // UIKit bounds so the SDK sees asset edges contained even at fractional scales.
        var size = _host.Bounds.Size;
        var bounds = new Rect(0, 0, size.Width, size.Height);
        var viewport = bounds.Intersect(_viewport);
        SetFrame(_clip, ToFrame(viewport));
        _clip.Hidden = viewport.Width <= 0 || viewport.Height <= 0;
        SetFrame(_adView, new CGRect(-viewport.X, -viewport.Y, size.Width, size.Height));

        var scale = (double)_host.TraitCollection.DisplayScale;
        if (scale <= 0)
        {
            scale = 1;
        }

        // Keep the converted edges one physical pixel inside the view. UIKit and
        // the SDK can round an edge differently when converting to screen coordinates.
        var assetArea = bounds.Deflate(1 / scale);
        for (var i = 0; i < _views.Length; i++)
        {
            var asset = assetArea.Intersect(_assetBounds[i]);
            SetFrame(_views[i], ToFrame(asset));
        }
    }

    public override void Dispose()
    {
        _host.ArrangeAd = null;
        if (_loader is not null)
        {
            _loader.Delegate = null;
        }

        _loaderDelegate?.Dispose();
        _loader?.Dispose();

        if (_ad is not null)
        {
            _ad.Delegate = null;
        }

        _adView.NativeAd = null;
        _adDelegate?.Dispose();
        _ad?.Dispose();

        _adView.RemoveFromSuperview();
        _adView.Dispose();
        foreach (var view in _views)
            view.Dispose();
        _clip.RemoveFromSuperview();
        _clip.Dispose();
    }

    private UIView CreateAsset(NativeAdAsset asset)
    {
        switch (asset)
        {
            case NativeAdAttribution attribution:
                return new UILabel
                {
                    AccessibilityIdentifier = "ad-attribution",
                    Text = string.IsNullOrWhiteSpace(attribution.Text) ? "Ad" : attribution.Text,
                    Font = UIFont.SystemFontOfSize(12)
                };
            case NativeAdIcon:
                return _adView.IconView = new UIImageView { ContentMode = UIViewContentMode.ScaleAspectFit };
            case NativeAdMedia:
                return _adView.MediaView = new MediaView
                {
                    ContentMode = UIViewContentMode.ScaleAspectFit,
                    ClipsToBounds = true
                };
            case NativeAdCallToAction:
                return _adView.CallToActionView = new UILabel
                {
                    AccessibilityIdentifier = "ad-call-to-action",
                    Lines = 0,
                    TextAlignment = UITextAlignment.Center,
                    TextColor = UIColor.White,
                    BackgroundColor = UIColor.SystemBlue,
                    UserInteractionEnabled = false
                };
            case NativeAdHeadline:
                return _adView.HeadlineView = new UILabel { Lines = 0, Font = UIFont.BoldSystemFontOfSize(17) };
            case NativeAdBody:
                return _adView.BodyView = new UILabel { Lines = 0 };
            case NativeAdAdvertiser:
                return _adView.AdvertiserView = new UILabel { Lines = 0 };
            default:
                throw new InvalidOperationException("Unsupported native ad asset.");
        }
    }

    private void Bind(NativeAd ad)
    {
        _ad = ad;
        try
        {
            SetText(_adView.HeadlineView, ad.Headline);
            SetText(_adView.CallToActionView, ad.CallToAction);
            SetText(_adView.BodyView, ad.Body);
            SetText(_adView.AdvertiserView, ad.Advertiser);

            var icon = (UIImageView)_adView.IconView;
            icon.Image = ad.Icon?.Image;
            icon.Hidden = icon.Image is null;

            _aspectRatio = ad.MediaContent.AspectRatio;
            _adDelegate = new AdDelegate(this);
            ad.Delegate = _adDelegate;
            AdLoaded();
        }
        catch (Exception error)
        {
            AdFailed(error.Message);
        }
    }

    private static void SetText(UIView? view, string? text)
    {
        if (view is not UILabel label)
        {
            return;
        }

        label.Text = text;
        label.Hidden = string.IsNullOrEmpty(text);
    }

    private static CGRect ToFrame(Rect bounds) => new(bounds.X, bounds.Y, bounds.Width, bounds.Height);

    private static void SetFrame(UIView view, CGRect frame)
    {
        if (view.Frame != frame)
        {
            view.Frame = frame;
        }
    }

    private sealed class AdHost : UIView
    {
        public Action? ArrangeAd { get; set; }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ArrangeAd?.Invoke();
        }
    }

    private sealed class LoaderDelegate(NativeAdViewiOS owner) : NativeAdLoaderDelegate
    {
        public override void DidReceiveNativeAd(AdLoader adLoader, NativeAd nativeAd) => owner.Bind(nativeAd);
        public override void DidFailToReceiveAd(AdLoader adLoader, NSError error) => owner.AdFailed(error.LocalizedDescription);
    }

    private sealed class AdDelegate(NativeAdViewiOS owner) : NativeAdDelegate
    {
        public override void DidRecordImpression(NativeAd nativeAd) => owner.AdImpression();
        public override void DidRecordClick(NativeAd nativeAd) => owner.AdClicked();
        public override void WillPresentScreen(NativeAd nativeAd) => owner.AdOpened();
        public override void DidDismissScreen(NativeAd nativeAd) => owner.AdClosed();
    }
}
