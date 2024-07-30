using System.Diagnostics;
using Avalonia.Controls;

namespace Jc.AdMob.Avalonia.Sample.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void BannerAd_OnOnAdFailedToLoad(object? sender, AdError e)
    {
        Debug.WriteLine(e.Message);
    }
}