using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Jc.AdMob.Avalonia.Sample.Views;

public partial class NativeAdPage : UserControl
{
    private readonly Action? _goBack;
    private TopLevel? _topLevel;

    public NativeAdPage() : this(video: false, goBack: null)
    {
    }

    internal NativeAdPage(bool video, Action? goBack)
    {
        InitializeComponent();
        _goBack = goBack;

        PageTitle.Text = video ? "Native video ad" : "Native image ad";
        AdPlacement.Children.Remove(video ? ImageAd : VideoAd);
        if (video)
        {
            VideoAd.UnitId = OperatingSystem.IsAndroid()
                ? "ca-app-pub-3940256099942544/1044960115"
                : "ca-app-pub-3940256099942544/2521693316";
        }

        if (!OperatingSystem.IsAndroid() && !OperatingSystem.IsIOS())
        {
            AdStatus.Text = "Native ads are available on Android and iOS.";
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _topLevel = TopLevel.GetTopLevel(this);
        if (_topLevel is not null)
        {
            _topLevel.BackRequested += BackClicked;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_topLevel is not null)
        {
            _topLevel.BackRequested -= BackClicked;
        }

        _topLevel = null;

        base.OnDetachedFromVisualTree(e);
    }

    private void BackClicked(object? sender, RoutedEventArgs e)
    {
        if (e.Handled || _goBack is null)
        {
            return;
        }

        e.Handled = true;
        _goBack();
    }

    private void NativeImageLoaded(object? sender, EventArgs e) => AdLoaded("Native image ad loaded.");

    private void NativeVideoLoaded(object? sender, EventArgs e) => AdLoaded("Native video ad loaded.");

    private void AdLoaded(string message)
    {
        AdStatus.IsVisible = false;
        Console.WriteLine(message);
    }

    private void NativeAdFailed(object? sender, AdError e)
    {
        AdStatus.Text = $"Native ad failed to load. {e.Message}";
        AdStatus.IsVisible = true;
        Console.WriteLine(AdStatus.Text);
    }

    private void NativeAdClicked(object? sender, EventArgs e) => Console.WriteLine("Native ad clicked.");

    private void NativeAdImpression(object? sender, EventArgs e) => Console.WriteLine("Native ad impression.");
}
