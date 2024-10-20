using System.Diagnostics;
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
    public ICommand ShowRewardedInterstitialAdCommand { get; }
    public ICommand ShowRewardedAdCommand { get; }
    
    public MainViewModel()
    {
        ResetConsentCommand = ReactiveCommand.Create(ResetConsent);
        ShowConsentCommand = ReactiveCommand.Create(ShowConsent);
        ShowInterstitialAdCommand = ReactiveCommand.Create(ShowInterstitialAd);
        ShowRewardedInterstitialAdCommand = ReactiveCommand.Create(ShowRewardedInterstitialAd);
        ShowRewardedAdCommand = ReactiveCommand.Create(ShowRewardedAd);
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
    
    private void ShowRewardedInterstitialAd()
    {
        var rewardedInterstitial = AdMob.Current.RewardedInterstitial.Create();
        rewardedInterstitial.OnAdLoaded += (_, _) => rewardedInterstitial.Show();
        rewardedInterstitial.OnUserEarnedReward += (_, reward) => Debug.WriteLine($"User earned reward: {reward.Amount} {reward.Type}");
    }
    
    private void ShowRewardedAd()
    {
        var rewarded = AdMob.Current.Rewarded.Create();
        rewarded.OnAdLoaded += (_, _) => rewarded.Show();
        rewarded.OnUserEarnedReward += (_, reward) => Debug.WriteLine($"User earned reward: {reward.Amount} {reward.Type}");
    }
}