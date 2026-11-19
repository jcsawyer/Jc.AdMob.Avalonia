using ISdkAd = Google.Android.Libraries.Ads.Mobile.Sdk.Rewarded.IRewardedAd;
using SdkAd = Google.Android.Libraries.Ads.Mobile.Sdk.Rewarded.RewardedAd;

namespace Jc.AdMob.Avalonia.Android;

public class RewardedAd : IRewardedAd
{
    private const string TestUnit = "ca-app-pub-3940256099942544/5224354917";
    private readonly FullScreenAd<ISdkAd> _ad;

    public event EventHandler? OnAdLoaded;
    public event EventHandler<AdError>? OnAdFailedToLoad;
    public event EventHandler? OnAdPresented;
    public event EventHandler<AdError>? OnAdFailedToPresent;
    public event EventHandler? OnAdImpression;
    public event EventHandler? OnAdClicked;
    public event EventHandler? OnAdClosed;
    public event EventHandler<RewardItem>? OnUserEarnedReward;

    public RewardedAd(AdMobOptions options, string? unitId = null)
        : this(unitId, options.TestDeviceIds)
    {
    }

    public RewardedAd(string? unitId, IReadOnlyCollection<string>? testDeviceIds = null)
    {
        _ad = new(
            string.IsNullOrWhiteSpace(unitId) ? TestUnit : unitId,
            testDeviceIds,
            SdkAd.Load,
            (ad, activity, callback) =>
            {
                ad.AdEventCallback = callback;
                ad.Show(activity, new RewardListener(reward => OnUserEarnedReward?.Invoke(this, reward)));
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
