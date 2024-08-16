using System.Windows.Input;
using ReactiveUI;

namespace Jc.AdMob.Avalonia.Sample.ViewModels;

public class MainViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static

    public ICommand ResetConsentCommand { get; }
    public ICommand ShowConsentCommand { get; }
    public ICommand ShowInterstitialAdCommand { get; }
    
    public MainViewModel()
    {
        ResetConsentCommand = ReactiveCommand.Create(ResetConsent);
        ShowConsentCommand = ReactiveCommand.Create(ShowConsent);
        ShowInterstitialAdCommand = ReactiveCommand.Create(ShowInterstitialAd);
    }

    private void ResetConsent()
    {
        AdMob.Current.Consent.Reset();
    }

    private void ShowConsent()
    {
        AdMob.Current.Consent.ShowConsent();
    }

    private void ShowInterstitialAd()
    {
        var interstitial = AdMob.Current.Interstitial.Create();
        interstitial.OnAdLoaded += (_, _) => interstitial.Show();
    }
}