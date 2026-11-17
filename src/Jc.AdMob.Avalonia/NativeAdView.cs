using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Jc.AdMob.Avalonia;

public sealed class NativeAdView : NativeControlHost
{
    public static readonly StyledProperty<string?> UnitIdProperty =
        AvaloniaProperty.Register<NativeAdView, string?>(nameof(UnitId));

    public static readonly StyledProperty<Control?> AdContentProperty =
        AvaloniaProperty.Register<NativeAdView, Control?>(nameof(AdContent));

    internal static Func<IPlatformHandle, IReadOnlyList<NativeAdAsset>, NativeAdPlatformView>? Implementation { get; set; }

    private NativeAdPlatformView? _platform;
    private IReadOnlyList<NativeAdAsset> _assets = [];
    private Rect[] _assetBounds = [];
    private int _mediaIndex;
    private readonly List<IDisposable> _hiddenAssets = [];
    private IAdConsent? _consent;
    private Rect _viewport;
    private bool _sdkReady;
    private bool _requested;
    private bool _loaded;
    private bool _registered;
    private bool _registrationQueued;
    private bool _failed;
    private int _hostVersion;

    public NativeAdView()
    {
        EffectiveViewportChanged += (_, e) =>
        {
            _viewport = e.EffectiveViewport;
            PositionAssets();
        };
        LayoutUpdated += (_, _) => PositionAssets();
    }

    public string? UnitId
    {
        get => GetValue(UnitIdProperty);
        set => SetValue(UnitIdProperty, value);
    }

    [Content]
    public Control? AdContent
    {
        get => GetValue(AdContentProperty);
        set => SetValue(AdContentProperty, value);
    }

    public event EventHandler? OnAdLoaded;
    public event EventHandler<AdError>? OnAdFailedToLoad;
    public event EventHandler? OnAdImpression;
    public event EventHandler? OnAdClicked;
    public event EventHandler? OnAdOpened;
    public event EventHandler? OnAdClosed;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != AdContentProperty)
        {
            return;
        }

        if (change.OldValue is Control oldContent)
        {
            Stop();
            VisualChildren.Remove(oldContent);
            LogicalChildren.Remove(oldContent);
        }

        if (change.NewValue is Control newContent)
        {
            LogicalChildren.Add(newContent);
            VisualChildren.Add(newContent);
        }

        InvalidateMeasure();
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var version = ++_hostVersion;
        _failed = false;
        _sdkReady = false;
        _requested = false;

        if (Implementation is null || Design.IsDesignMode)
        {
            return base.CreateNativeControlCore(parent);
        }

        try
        {
            _assets = GetAssets(AdContent);
            _assetBounds = new Rect[_assets.Count];
            var platform = Implementation(parent, _assets);
            _platform = platform;
            platform.Loaded += () => Dispatch(platform, AdLoaded);
            platform.Failed += error => Dispatch(platform, () => Fail(error));
            platform.Impression += () => Dispatch(platform, () => OnAdImpression?.Invoke(this, EventArgs.Empty));
            platform.Clicked += () => Dispatch(platform, () => OnAdClicked?.Invoke(this, EventArgs.Empty));
            platform.Opened += () => Dispatch(platform, () => OnAdOpened?.Invoke(this, EventArgs.Empty));
            platform.Closed += () => Dispatch(platform, () => OnAdClosed?.Invoke(this, EventArgs.Empty));

            // The attachment and its parent view controller are available after creation returns.
            Dispatcher.UIThread.Post(() =>
            {
                if (ReferenceEquals(_platform, platform))
                {
                    WaitForAds();
                }
            });

            return platform.Handle;
        }
        catch (Exception error)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (version == _hostVersion)
                {
                    Fail(new AdError(error.Message));
                }
            });
            return base.CreateNativeControlCore(parent);
        }
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        Stop();
        base.DestroyNativeControlCore(control);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (!_loaded || AdContent is null)
        {
            return default;
        }

        AdContent.Measure(availableSize);
        return AdContent.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_loaded)
        {
            AdContent?.Arrange(new Rect(finalSize));
        }

        return finalSize;
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new AdMobAutomationPeer(this);

    private void PositionAssets()
    {
        if (!_loaded || _platform is null || !IsArrangeValid)
        {
            return;
        }

        var hostBounds = new Rect(Bounds.Size);
        var viewport = hostBounds.Intersect(_viewport);
        for (var i = 0; i < _assets.Count; i++)
        {
            var asset = _assets[i];
            // The SDK checks the asset frames themselves. Clipping the scroll viewport
            // does not keep a rounded or overflowing Avalonia frame inside the ad.
            _assetBounds[i] = asset.IsEffectivelyVisible && asset.TranslatePoint(default, this) is { } origin
                ? hostBounds.Intersect(new Rect(origin, asset.Bounds.Size))
                : default;
        }

        var media = _assetBounds[_mediaIndex];
        var mediaFits = media.Width >= 120 && media.Height >= 120;
        _platform.Arrange(Bounds.Size, mediaFits ? viewport : default, _assetBounds);

        if (_registered || !mediaFits)
        {
            return;
        }

        if (!_registrationQueued)
        {
            _registrationQueued = true;
            var platform = _platform;
            Dispatcher.UIThread.Post(() =>
            {
                if (!ReferenceEquals(_platform, platform))
                {
                    return;
                }

                _registrationQueued = false;
                RegisterAd(platform);
            }, DispatcherPriority.Loaded);
        }
    }

    private void RegisterAd(NativeAdPlatformView platform)
    {
        // NativeControlHost updates the platform attachment separately from Avalonia layout.
        // Synchronize its outer frame before the SDK sees the already arranged assets.
        if (!this.IsAttachedToVisualTree() || !IsArrangeValid ||
            _assetBounds[_mediaIndex].Width < 120 || _assetBounds[_mediaIndex].Height < 120 ||
            !TryUpdateNativeControlPosition())
        {
            return;
        }

        _registered = true;
        try
        {
            platform.RegisterAd();
            Dispatch(platform, () => OnAdLoaded?.Invoke(this, EventArgs.Empty));
        }
        catch (Exception error)
        {
            Dispatch(platform, () => Fail(new AdError(error.Message)));
        }
    }

    private void WaitForAds()
    {
        _consent = AdMob.Current.Consent;
        _consent.OnConsentProvided += ConsentProvided;
        AdMob.Current.OnAdsInitialized += AdsInitialized;
    }

    private void AdsInitialized(object? sender, EventArgs e)
    {
        var platform = _platform;
        if (platform is not null)
        {
            Dispatch(platform, () =>
            {
                _sdkReady = true;
                TryLoad();
            });
        }
    }

    private void ConsentProvided(object? sender, EventArgs e)
    {
        var platform = _platform;
        if (platform is not null)
        {
            Dispatch(platform, TryLoad);
        }
    }

    private void TryLoad()
    {
        if (!_sdkReady || _requested || !this.IsAttachedToVisualTree() || !AdMob.Current.CanShowAds)
        {
            return;
        }

        _requested = true;
        Unsubscribe();
        try
        {
            _platform?.Load(UnitId);
        }
        catch (Exception error)
        {
            Fail(new AdError(error.Message));
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_platform is { } platform)
        {
            Dispatch(platform, TryLoad);
        }
    }

    private void Dispatch(NativeAdPlatformView platform, Action action)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (ReferenceEquals(_platform, platform) && !_failed)
            {
                action();
            }
        });
    }

    private void AdLoaded()
    {
        if (_loaded || _platform is null)
        {
            return;
        }

        for (var i = 0; i < _assets.Count; i++)
        {
            var index = i;
            var asset = _assets[i];
            if (asset is NativeAdMedia)
            {
                _mediaIndex = i;
            }

            var platform = _platform;
            if (platform.HasAsset(i))
            {
                asset.MeasureAsset = size => platform.Measure(index, size);
            }
            else
            {
                _hiddenAssets.Add(asset.SetValue(IsVisibleProperty, false, BindingPriority.Animation)!);
            }

            asset.InvalidateMeasure();
        }

        _loaded = true;
        InvalidateMeasure();
    }

    private void Fail(AdError error)
    {
        if (_failed)
        {
            return;
        }

        _failed = true;
        Stop();
        OnAdFailedToLoad?.Invoke(this, error);
    }

    private void Stop()
    {
        _hostVersion++;
        Unsubscribe();
        _loaded = false;
        _registered = false;
        _registrationQueued = false;
        var platform = _platform;
        _platform = null;
        platform?.Dispose();

        foreach (var hiddenAsset in _hiddenAssets)
            hiddenAsset.Dispose();
        _hiddenAssets.Clear();

        foreach (var asset in _assets)
        {
            asset.MeasureAsset = null;
            asset.InvalidateMeasure();
        }

        _assets = [];
        _assetBounds = [];
        InvalidateMeasure();
    }

    private void Unsubscribe()
    {
        AdMob.Current.OnAdsInitialized -= AdsInitialized;
        if (_consent is not null)
        {
            _consent.OnConsentProvided -= ConsentProvided;
        }

        _consent = null;
    }

    private static IReadOnlyList<NativeAdAsset> GetAssets(Control? content)
    {
        var assets = new List<NativeAdAsset>();
        var types = new HashSet<Type>();
        Visit(content);

        if (!types.Contains(typeof(NativeAdAttribution)) || !types.Contains(typeof(NativeAdHeadline)) ||
            !types.Contains(typeof(NativeAdIcon)) || !types.Contains(typeof(NativeAdMedia)) ||
            !types.Contains(typeof(NativeAdCallToAction)))
        {
            throw new InvalidOperationException("AdContent requires attribution, headline, icon, media, and call-to-action controls.");
        }

        return assets;

        void Visit(Control? control)
        {
            switch (control)
            {
                case NativeAdAsset asset:
                    if (!types.Add(asset.GetType()))
                    {
                        throw new InvalidOperationException($"AdContent contains more than one {asset.GetType().Name}.");
                    }

                    assets.Add(asset);
                    break;
                case Grid or StackPanel:
                    foreach (var child in ((Panel)control).Children)
                        Visit(child);
                    break;
                default:
                    throw new InvalidOperationException("AdContent supports Grid, StackPanel, and native ad asset controls.");
            }
        }
    }
}
