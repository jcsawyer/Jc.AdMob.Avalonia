using Android.Gms.Ads;

namespace Jc.AdMob.Avalonia.Android;

internal sealed class AppOpenAdCallbacks : AppOpenAdLoadCallback
{
    public event EventHandler<global::Android.Gms.Ads.AppOpen.AppOpenAd> WhenAdLoaded;
    public event EventHandler<LoadAdError> WhenAdFailedToLoaded;

    public override void OnAdLoaded(global::Android.Gms.Ads.AppOpen.AppOpenAd appOpenAd)
    {
        base.OnAdLoaded(appOpenAd);
        WhenAdLoaded?.Invoke(this, appOpenAd);
    }

    public override void OnAdFailedToLoad(LoadAdError error)
    {
        base.OnAdFailedToLoad(error);
        WhenAdFailedToLoaded?.Invoke(this, error);
    }
}