using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Jc.AdMob.Avalonia.Sample.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void ShowNativeImageAd(object? sender, RoutedEventArgs e) => ShowNativeAd(video: false);

    private void ShowNativeVideoAd(object? sender, RoutedEventArgs e) => ShowNativeAd(video: true);

    private void ShowNativeAd(bool video)
    {
        var mainPage = Content;
        Content = new NativeAdPage(video, () => Content = mainPage);
    }

    private void BannerAd_OnOnAdFailedToLoad(object? sender, AdError e)
    {
        Debug.WriteLine(e.Message);
    }
}
