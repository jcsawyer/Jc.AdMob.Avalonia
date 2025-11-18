using System;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Platform;

namespace Jc.AdMob.Avalonia;

public sealed class BannerAd : NativeControlHost
{
    public static INativeBannerAd? Implementation { get; set; }

    static BannerAd()
    {
    }

    public event EventHandler? OnAdLoaded;
    public event EventHandler<AdError>? OnAdFailedToLoad;
    public event EventHandler? OnAdImpression;
    public event EventHandler? OnAdClicked;
    public event EventHandler? OnAdOpened;
    public event EventHandler? OnAdClosed;
    public event EventHandler? OnAdSwiped;
    
    public string? UnitId { get; set; }

    public AdSize AdSize { get; set; } = (AdSize)0xFF;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        return Implementation?.CreateControl(UnitId, this, parent, () => base.CreateNativeControlCore(parent))
               ?? base.CreateNativeControlCore(parent);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
        => new AdMobAutomationPeer(this);

    internal void AdLoaded(object? sender, EventArgs e) => OnAdLoaded?.Invoke(sender, e);
    internal void AdFailedToLoad(object? sender, AdError e) => OnAdFailedToLoad?.Invoke(sender, e);
    internal void AdImpression(object? sender, EventArgs e) => OnAdImpression?.Invoke(sender, e);
    internal void AdClicked(object? sender, EventArgs e) => OnAdClicked?.Invoke(sender, e);
    internal void AdSwiped(object? sender, EventArgs e) => OnAdSwiped?.Invoke(sender, e);
    internal void AdOpened(object? sender, EventArgs e) => OnAdOpened?.Invoke(sender, e);
    internal void AdClosed(object? sender, EventArgs e) => OnAdClosed?.Invoke(sender, e);
}