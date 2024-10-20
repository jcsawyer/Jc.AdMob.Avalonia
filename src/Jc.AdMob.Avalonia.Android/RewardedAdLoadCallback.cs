using Android.Gms.Ads;
using Android.Gms.Ads.Rewarded;
using Android.Runtime;

namespace Jc.AdMob.Avalonia.Android;

internal abstract class RewardedAdLoadCallback : global::Android.Gms.Ads.Rewarded.RewardedAdLoadCallback
{
    [Register("onAdLoaded", "(Lcom/google/android/gms/ads/rewarded/RewardedAd;)V", "GetOnAdLoadedHandler")]
    public virtual void OnAdLoaded(global::Android.Gms.Ads.Rewarded.RewardedAd rewardedAd)
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
        var @this = GetObject<RewardedAdLoadCallback>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
        var result = GetObject<global::Android.Gms.Ads.Rewarded.RewardedAd>(native_p0, JniHandleOwnership.DoNotTransfer);
        @this.OnAdLoaded(result);
    }
}