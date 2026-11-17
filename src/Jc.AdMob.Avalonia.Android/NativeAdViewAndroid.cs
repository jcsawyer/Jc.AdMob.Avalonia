using Android.Content;
using Android.Gms.Ads;
using Android.Gms.Ads.NativeAd;
using Android.Views;
using Android.Widget;
using Avalonia;
using Avalonia.Android;
using Avalonia.Platform;
using SdkAd = Android.Gms.Ads.NativeAd.NativeAd;
using SdkView = Android.Gms.Ads.NativeAd.NativeAdView;

namespace Jc.AdMob.Avalonia.Android;

internal sealed class NativeAdViewAndroid : NativeAdPlatformView
{
    private const string TestUnit = "ca-app-pub-3940256099942544/2247696110";

    private readonly Context _context;
    private readonly AndroidViewControlHandle _handle;
    private readonly FrameLayout _clip;
    private readonly SdkView _adView;
    private readonly View[] _views;
    private readonly float _density;
    private AdLoader? _loader;
    private Listener? _listener;
    private SdkAd? _ad;
    private bool _disposed;
    private double _aspectRatio;

    public NativeAdViewAndroid(IPlatformHandle parent, IReadOnlyList<NativeAdAsset> assets)
    {
        _context = ((AndroidViewControlHandle)parent).View.Context!;
        _density = _context.Resources?.DisplayMetrics?.Density ?? 1;
        var host = new FrameLayout(_context);
        _handle = new AndroidViewControlHandle(host);
        _adView = new SdkView(_context);
        _adView.SetBackgroundColor(global::Android.Graphics.Color.White);
        _clip = new FrameLayout(_context);
        host.AddView(_clip, new FrameLayout.LayoutParams(0, 0));
        _clip.AddView(_adView, new FrameLayout.LayoutParams(0, 0));
        var layout = new FrameLayout(_context);
        _adView.AddView(layout, new FrameLayout.LayoutParams(-1, -1));

        _views = assets.Select(asset =>
        {
            var view = CreateAsset(asset);
            layout.AddView(view, new FrameLayout.LayoutParams(0, 0));
            return view;
        }).ToArray();
    }

    public override IPlatformHandle Handle => _handle;

    public override void Load(string? unitId)
    {
        var options = AdMob.Current.Options;
        var configuration = MobileAds.RequestConfiguration.ToBuilder()
            .SetTagForChildDirectedTreatment(options.TagForChildDirectedTreatment ? 1 : -1)
            .SetTagForUnderAgeOfConsent(options.TagForUnderAgeOfConsent ? 1 : -1);
        if (options.TestDeviceIds is not null)
        {
            configuration.SetTestDeviceIds(options.TestDeviceIds.ToList());
        }

        MobileAds.RequestConfiguration = configuration.Build();

        _listener = new Listener(this);
        _loader = new AdLoader.Builder(_context, string.IsNullOrWhiteSpace(unitId) ? TestUnit : unitId)
            .ForNativeAd(_listener)
            .WithAdListener(_listener)
            .Build();
        _loader.LoadAd(AdRequest.GetRequest(typeof(global::Google.Ads.Mediation.Admob.AdMobAdapter)));
    }

    public override void RegisterAd() => _adView.SetNativeAd(_ad!);

    public override bool HasAsset(int index) => _views[index].Visibility == ViewStates.Visible;

    public override Size Measure(int index, Size available)
    {
        var view = _views[index];
        if (view is MediaView)
        {
            return MediaSize(available, _aspectRatio);
        }

        if (view is ImageView)
        {
            return new Size(40, 40);
        }

        view.Measure(MeasureSpec(available.Width), MeasureSpec(available.Height));
        return new Size(view.MeasuredWidth / _density, view.MeasuredHeight / _density);
    }

    public override void Arrange(Size size, Rect viewport, IReadOnlyList<Rect> assetBounds)
    {
        Place(_clip, viewport);
        Place(_adView, new Rect(-viewport.X, -viewport.Y, size.Width, size.Height));
        _clip.Visibility = viewport.Width > 0 && viewport.Height > 0 ? ViewStates.Visible : ViewStates.Invisible;

        for (var i = 0; i < _views.Length; i++)
            Place(_views[i], assetBounds[i]);
    }

    private void Place(View view, Rect bounds)
    {
        var left = Pixels(bounds.X);
        var top = Pixels(bounds.Y);
        var width = Pixels(bounds.Right) - left;
        var height = Pixels(bounds.Bottom) - top;
        if (view.LayoutParameters is FrameLayout.LayoutParams current &&
            current.LeftMargin == left && current.TopMargin == top &&
            current.Width == width && current.Height == height)
        {
            return;
        }

        view.LayoutParameters = new FrameLayout.LayoutParams(width, height, GravityFlags.Top | GravityFlags.Left)
        {
            LeftMargin = left,
            TopMargin = top
        };
    }

    public override void Dispose()
    {
        _disposed = true;
        if (_listener is not null)
        {
            _listener.Owner = null;
        }

        _ad?.Destroy();
        _ad?.Dispose();
        _ad = null;
        _loader?.Dispose();
        _loader = null;
        // AdLoader has no cancellation API. The listener still destroys a late ad.
        _listener = null;
        _clip.RemoveView(_adView);
        _clip.Visibility = ViewStates.Gone;
        _adView.Destroy();
        _adView.Dispose();
        foreach (var view in _views)
            view.Dispose();
        _clip.Dispose();
    }

    private View CreateAsset(NativeAdAsset asset)
    {
        switch (asset)
        {
            case NativeAdAttribution attribution:
                var badge = new TextView(_context)
                {
                    Text = string.IsNullOrWhiteSpace(attribution.Text) ? "Ad" : attribution.Text
                };
                badge.SetTextColor(global::Android.Graphics.Color.Black);
                badge.SetMinWidth(Pixels(15));
                badge.SetMinHeight(Pixels(15));
                return badge;
            case NativeAdIcon:
                return _adView.IconView = new ImageView(_context);
            case NativeAdMedia:
                var media = new MediaView(_context);
                media.SetImageScaleType(ImageView.ScaleType.FitCenter);
                _adView.MediaView = media;
                return media;
            case NativeAdCallToAction:
                return _adView.CallToActionView = new Button(_context);
            case NativeAdHeadline:
                return _adView.HeadlineView = CreateLabel();
            case NativeAdBody:
                return _adView.BodyView = CreateLabel();
            case NativeAdAdvertiser:
                return _adView.AdvertiserView = CreateLabel();
            default:
                throw new InvalidOperationException("Unsupported native ad asset.");
        }
    }

    private TextView CreateLabel()
    {
        var label = new TextView(_context);
        label.SetTextColor(global::Android.Graphics.Color.Black);
        return label;
    }

    private void Bind(SdkAd ad)
    {
        if (_disposed || _ad is not null)
        {
            ad.Destroy();
            ad.Dispose();
            return;
        }

        _ad = ad;
        try
        {
            SetText(_adView.HeadlineView, ad.Headline);
            SetText(_adView.CallToActionView, ad.CallToAction);
            SetText(_adView.BodyView, ad.Body);
            SetText(_adView.AdvertiserView, ad.Advertiser);

            var icon = (ImageView)_adView.IconView!;
            var drawable = ad.Icon?.Drawable;
            icon.SetImageDrawable(drawable);
            icon.Visibility = drawable is null ? ViewStates.Gone : ViewStates.Visible;

            var media = _adView.MediaView!;
            media.MediaContent = ad.MediaContent;
            _aspectRatio = ad.MediaContent?.AspectRatio ?? 0;
            AdLoaded();
        }
        catch (Exception error)
        {
            AdFailed(error.Message);
        }
    }

    private static void SetText(View? view, string? text)
    {
        if (view is not TextView label)
        {
            return;
        }

        label.Text = text;
        label.Visibility = string.IsNullOrEmpty(text) ? ViewStates.Gone : ViewStates.Visible;
    }

    private int Pixels(double logical) => (int)Math.Round(logical * _density);
    private int MeasureSpec(double logical) => double.IsFinite(logical)
        ? View.MeasureSpec.MakeMeasureSpec(Pixels(logical), MeasureSpecMode.AtMost)
        : View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);

    private sealed class Listener(NativeAdViewAndroid owner) : AdListener, SdkAd.IOnNativeAdLoadedListener
    {
        public NativeAdViewAndroid? Owner { get; set; } = owner;

        public void OnNativeAdLoaded(SdkAd ad) => global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (Owner is { } active)
            {
                active.Bind(ad);
            }
            else
            {
                ad.Destroy();
                ad.Dispose();
            }
        });

        public override void OnAdFailedToLoad(LoadAdError error) => Owner?.AdFailed(error.Message);
        public override void OnAdImpression() => Owner?.AdImpression();
        public override void OnAdClicked() => Owner?.AdClicked();
        public override void OnAdOpened() => Owner?.AdOpened();
        public override void OnAdClosed() => Owner?.AdClosed();
    }
}
