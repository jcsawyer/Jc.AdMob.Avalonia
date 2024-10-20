using System;

namespace Jc.AdMob.Avalonia;

public interface IRewardedAd
{
    event EventHandler? OnAdLoaded;
    event EventHandler<AdError>? OnAdFailedToLoad;
    event EventHandler? OnAdPresented;
    event EventHandler<AdError>? OnAdFailedToPresent;
    event EventHandler? OnAdImpression;
    event EventHandler? OnAdClicked;
    event EventHandler? OnAdClosed;
    event EventHandler<RewardItem> OnUserEarnedReward;

    internal void Load();
    void Show();
}