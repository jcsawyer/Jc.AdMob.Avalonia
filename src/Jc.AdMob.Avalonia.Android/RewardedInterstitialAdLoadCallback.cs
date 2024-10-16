using Android.Gms.Ads;
using Android.Gms.Ads.Rewarded;
using Android.Runtime;

namespace Jc.AdMob.Avalonia.Android;

/// <summary>
/// Temporary workaround until the bindings libraries are updated
/// Source: https://github.com/xamarin/GooglePlayServicesComponents/issues/425
/// </summary>
internal abstract class RewardedInterstitialAdLoadCallback : global::Android.Gms.Ads.RewardedInterstitial.RewardedInterstitialAdLoadCallback
{
    [Register("onAdLoaded", "(Lcom/google/android/gms/ads/rewardedinterstitial/RewardedInterstitialAd;)V", "GetOnAdLoadedHandler")]
    public virtual void OnAdLoaded(global::Android.Gms.Ads.RewardedInterstitial.RewardedInterstitialAd rewardedInterstitialAd)
    {
    }
    
    private static Delegate cb_onAdLoaded;
    
    private static Delegate GetOnAdLoadedHandler()
    {
        if (cb_onAdLoaded is null)
            cb_onAdLoaded = JNINativeWrapper.CreateDelegate((Action<IntPtr, IntPtr, IntPtr>)n_onAdLoaded);
        return cb_onAdLoaded;
    }
    private static void n_onAdLoaded(IntPtr jnienv, IntPtr native__this, IntPtr native_p0)
    {
        var @this = GetObject<RewardedInterstitialAdLoadCallback>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
        var result = GetObject<global::Android.Gms.Ads.RewardedInterstitial.RewardedInterstitialAd>(native_p0, JniHandleOwnership.DoNotTransfer);
        @this.OnAdLoaded(result);
    }
}