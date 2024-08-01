using Android.Gms.Ads;
using Android.Runtime;

namespace Jc.AdMob.Avalonia.Android;

internal sealed class InterstitialAdCallbacks : InterstitialAdLoadCallback
{
    public event EventHandler<global::Android.Gms.Ads.Interstitial.InterstitialAd> WhenAdLoaded;
    public event EventHandler<LoadAdError> WhenAdFailedToLoaded;

    [Register("onAdLoaded", "(Lcom/google/android/gms/ads/interstitial/InterstitialAd;)V", "GetOnAdLoadedHandler")]
    public override void OnAdLoaded(global::Android.Gms.Ads.Interstitial.InterstitialAd interstitialAd)
    {
        base.OnAdLoaded(interstitialAd);
        WhenAdLoaded?.Invoke(this, interstitialAd);
    }

    public override void OnAdFailedToLoad(LoadAdError error)
    {
        base.OnAdFailedToLoad(error);
        WhenAdFailedToLoaded?.Invoke(this, error);
    }
}