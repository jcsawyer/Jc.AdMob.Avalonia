using ISdkAd = Google.Android.Libraries.Ads.Mobile.Sdk.Interstitial.IInterstitialAd;
using SdkAd = Google.Android.Libraries.Ads.Mobile.Sdk.Interstitial.InterstitialAd;

namespace Jc.AdMob.Avalonia.Android;

public sealed class InterstitialAd : IInterstitialAd
{
    private const string TestUnit = "ca-app-pub-3940256099942544/1033173712";
    private readonly FullScreenAd<ISdkAd> _ad;

    public event EventHandler? OnAdLoaded;
    public event EventHandler<AdError>? OnAdFailedToLoad;
    public event EventHandler? OnAdPresented;
    public event EventHandler<AdError>? OnAdFailedToPresent;
    public event EventHandler? OnAdImpression;
    public event EventHandler? OnAdClicked;
    public event EventHandler? OnAdClosed;

    internal InterstitialAd(AdMobOptions options, string? unitId = null)
    {
        _ad = new(
            string.IsNullOrWhiteSpace(unitId) ? TestUnit : unitId,
            options.TestDeviceIds,
            SdkAd.Load,
            static (ad, activity, callback) =>
            {
                ad.AdEventCallback = callback;
                ad.Show(activity);
            },
            () => OnAdLoaded?.Invoke(this, EventArgs.Empty),
            error => OnAdFailedToLoad?.Invoke(this, error),
            error => OnAdFailedToPresent?.Invoke(this, error));
    }

    public void Load() => _ad.Load();

    public void Show() => _ad.Show(new FullScreenContentCallback(
        () => OnAdPresented?.Invoke(this, EventArgs.Empty),
        error => OnAdFailedToPresent?.Invoke(this, error),
        () => OnAdImpression?.Invoke(this, EventArgs.Empty),
        () => OnAdClicked?.Invoke(this, EventArgs.Empty),
        () => OnAdClosed?.Invoke(this, EventArgs.Empty)));
}
