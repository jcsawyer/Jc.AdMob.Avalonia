using Android.Runtime;

namespace Jc.AdMob.Avalonia.Android;

internal abstract class AppOpenAdLoadCallback : global::Android.Gms.Ads.AppOpen.AppOpenAd.AppOpenAdLoadCallback
{
    [Register("onAdLoaded", "(Lcom/google/android/gms/ads/appopen/AppOpenAd;)V", "GetOnAdLoadedHandler")]
    public virtual void OnAdLoaded(global::Android.Gms.Ads.AppOpen.AppOpenAd appOpenAd)
    {
    }

    private static Delegate? cb_onAdLoaded;

#pragma warning disable IDE0051 // Remove unused private members
    private static Delegate GetOnAdLoadedHandler()
#pragma warning restore IDE0051 // Remove unused private members
    {
        if (cb_onAdLoaded is null)
            cb_onAdLoaded = JNINativeWrapper.CreateDelegate(n_onAdLoaded);
        return cb_onAdLoaded;
    }

    private static void n_onAdLoaded(IntPtr jnienv, IntPtr native__this, IntPtr native_p0)
    {
        AppOpenAdLoadCallback @this = GetObject<AppOpenAdLoadCallback>(jnienv, native__this, JniHandleOwnership.DoNotTransfer)!;
        global::Android.Gms.Ads.AppOpen.AppOpenAd result = GetObject<global::Android.Gms.Ads.AppOpen.AppOpenAd>(native_p0, JniHandleOwnership.DoNotTransfer)!;
        @this.OnAdLoaded(result);
    }
}