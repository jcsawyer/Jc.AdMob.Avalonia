using Android.App;
using Android.Runtime;
using Avalonia.Threading;
using Google.Android.Libraries.Ads.Mobile.Sdk.Common;
using SdkRequest = Google.Android.Libraries.Ads.Mobile.Sdk.Common.AdRequest;

namespace Jc.AdMob.Avalonia.Android;

internal sealed class FullScreenAd<T>(
    string unitId,
    IReadOnlyCollection<string>? testDeviceIds,
    Action<SdkRequest, IAdLoadCallback> load,
    Action<T, Activity, FullScreenContentCallback> show,
    Action loaded,
    Action<AdError> loadFailed,
    Action<AdError> showFailed) where T : class, IAd
{
    private T? _ad;
    private bool _loading;

    // The public factories start loading before returning the wrapper to its caller.
    internal void Load() => Dispatcher.UIThread.Post(StartLoad);

    private async void StartLoad()
    {
        if (_loading)
        {
            return;
        }

        if (AdMob.Current.Consent is not AdConsentAndroid consent)
        {
            loadFailed(new AdError("Call UseAdMob before loading ads on Android."));
            return;
        }

        if (!AdMob.Current.CanShowAds)
        {
            loadFailed(new AdError("Consent has not been granted."));
            return;
        }

        _loading = true;
        var initializationError = await consent.SdkInitialization;
        if (initializationError is not null || !AdMob.Current.CanShowAds)
        {
            FailLoad(initializationError ?? new AdError("Consent has not been granted."));
            return;
        }

        try
        {
            consent.AddTestDevices(testDeviceIds);
            using var builder = new SdkRequest.Builder(unitId);
            AdRequest.ApplyConsent(builder);
            load(builder.Build(), new AdLoadCallback(SetLoadedAd, FailLoad));
        }
        catch (Exception error)
        {
            FailLoad(new AdError(error.Message));
        }
    }

    private void SetLoadedAd(Java.Lang.Object? result)
    {
        if (result?.JavaCast<T>() is not { } ad)
        {
            FailLoad(new AdError("The ad SDK returned no ad."));
            return;
        }

        _loading = false;
        _ad?.Destroy();
        _ad = ad;
        loaded();
    }

    private void FailLoad(AdError error)
    {
        _loading = false;
        loadFailed(error);
    }

    internal void Show(FullScreenContentCallback callback)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Show(callback));
            return;
        }

        if (_ad is not { } ad)
        {
            return;
        }

        if (!AdMob.Current.CanShowAds)
        {
            _ad = null;
            ad.Destroy();
            showFailed(new AdError("Consent has not been granted."));
            return;
        }

        if (ActivityProvider.CurrentActivity is not { IsFinishing: false, IsDestroyed: false } activity)
        {
            showFailed(new AdError("No active Activity is available to present the ad."));
            return;
        }

        _ad = null;
        try
        {
            show(ad, activity, callback);
        }
        catch (Exception error)
        {
            ad.Destroy();
            showFailed(new AdError(error.Message));
        }
    }
}

internal sealed class AdLoadCallback(Action<Java.Lang.Object?> loaded, Action<AdError> failed)
    : Java.Lang.Object, IAdLoadCallback
{
    public void OnAdLoaded(Java.Lang.Object? ad) => Dispatcher.UIThread.Post(() => loaded(ad));

    public void OnAdFailedToLoad(LoadAdError error)
    {
        var message = error.Message;
        Dispatcher.UIThread.Post(() => failed(new AdError(message)));
    }
}
