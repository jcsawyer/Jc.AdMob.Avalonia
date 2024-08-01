using System.Diagnostics;
using System.Windows.Input;
using ReactiveUI;

namespace Jc.AdMob.Avalonia.Sample.ViewModels;

public class MainViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static

    public ICommand ShowInterstitialAdCommand { get; }
    
    public MainViewModel()
    {
        ShowInterstitialAdCommand = ReactiveCommand.Create(ShowInterstitialAd);
    }

    private void ShowInterstitialAd()
    {
        var interstitial = App.InterstitialAd.Create();
        interstitial.OnAdLoaded += (_, _) => interstitial.Show();
        interstitial.OnAdFailedToLoad += (s, e) => Debug.WriteLine(e);
    }
}