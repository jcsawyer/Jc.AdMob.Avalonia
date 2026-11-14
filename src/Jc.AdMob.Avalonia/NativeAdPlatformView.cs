using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Platform;

namespace Jc.AdMob.Avalonia;

internal abstract class NativeAdPlatformView : IDisposable
{
    public event Action? Loaded;
    public event Action<AdError>? Failed;
    public event Action? Impression;
    public event Action? Clicked;
    public event Action? Opened;
    public event Action? Closed;

    public abstract IPlatformHandle Handle { get; }
    public abstract void Load(string? unitId);
    public abstract bool HasAsset(int index);
    public abstract Size Measure(int index, Size available);
    public abstract void Arrange(Size size, Rect viewport, IReadOnlyList<Rect> assetBounds);
    public abstract void Dispose();

    protected void AdLoaded() => Loaded?.Invoke();
    protected void AdFailed(string message) => Failed?.Invoke(new AdError(message));
    protected void AdImpression() => Impression?.Invoke();
    protected void AdClicked() => Clicked?.Invoke();
    protected void AdOpened() => Opened?.Invoke();
    protected void AdClosed() => Closed?.Invoke();

    public abstract void RegisterAd();

    protected static Size MediaSize(Size available, double aspectRatio)
    {
        var width = double.IsFinite(available.Width) ? Math.Max(120, available.Width) : 320;
        var height = Math.Max(120, width / (aspectRatio > 0 ? aspectRatio : 16d / 9));
        return new Size(width, height);
    }
}
