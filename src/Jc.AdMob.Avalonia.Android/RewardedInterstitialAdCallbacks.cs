using Android.Gms.Ads;
using Android.Gms.Ads.Rewarded;
using Android.Runtime;

namespace Jc.AdMob.Avalonia.Android;

internal sealed class RewardedInterstitialAdCallbacks : RewardedInterstitialAdLoadCallback, IOnUserEarnedRewardListener
{
    public event EventHandler<global::Android.Gms.Ads.RewardedInterstitial.RewardedInterstitialAd>? WhenAdLoaded;
    public event EventHandler<LoadAdError>? WhenAdFailedToLoad;
    public event EventHandler<IRewardItem>? WhenUserEarnedReward;
    
    [Register("onAdLoaded", "(Lcom/google/android/gms/ads/rewardedinterstitial/RewardedInterstitialAd;)V", "GetOnAdLoadedHandler")]
    public override void OnAdLoaded(global::Android.Gms.Ads.RewardedInterstitial.RewardedInterstitialAd rewardedInterstitialAd)
    {
        base.OnAdLoaded(rewardedInterstitialAd);
        WhenAdLoaded?.Invoke(this, rewardedInterstitialAd);
    }

    public override void OnAdFailedToLoad(LoadAdError error)
    {
        base.OnAdFailedToLoad(error);
        WhenAdFailedToLoad?.Invoke(this, error);
    }

    public void OnUserEarnedReward(IRewardItem reward)
    {
        WhenUserEarnedReward?.Invoke(this, reward);
    }
}