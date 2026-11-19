using Avalonia.Threading;
using Google.Android.Libraries.Ads.Mobile.Sdk.AppOpen;
using Google.Android.Libraries.Ads.Mobile.Sdk.Common;
using Google.Android.Libraries.Ads.Mobile.Sdk.Interstitial;
using Google.Android.Libraries.Ads.Mobile.Sdk.Rewarded;
using Google.Android.Libraries.Ads.Mobile.Sdk.RewardedInterstitial;

namespace Jc.AdMob.Avalonia.Android;

internal sealed class FullScreenContentCallback(
    Action presented,
    Action<AdError> failed,
    Action impression,
    Action clicked,
    Action closed)
    : Java.Lang.Object, IAppOpenAdEventCallback, IInterstitialAdEventCallback,
      IRewardedAdEventCallback, IRewardedInterstitialAdEventCallback
{
    public void OnAdShowedFullScreenContent() => Dispatcher.UIThread.Post(presented);

    public void OnAdFailedToShowFullScreenContent(FullScreenContentError error)
    {
        var message = error.Message;
        Dispatcher.UIThread.Post(() => failed(new AdError(message)));
    }

    public void OnAdImpression() => Dispatcher.UIThread.Post(impression);
    public void OnAdClicked() => Dispatcher.UIThread.Post(clicked);
    public void OnAdDismissedFullScreenContent() => Dispatcher.UIThread.Post(closed);
}

internal sealed class RewardListener(Action<RewardItem> earned)
    : Java.Lang.Object, IOnUserEarnedRewardListener
{
    private int _delivered;

    public void OnUserEarnedReward(IRewardItem reward)
    {
        if (Interlocked.Exchange(ref _delivered, 1) == 0)
        {
            var item = new RewardItem(reward.Amount, reward.Type);
            Dispatcher.UIThread.Post(() => earned(item));
        }
    }
}
