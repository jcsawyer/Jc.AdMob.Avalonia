using System;

namespace Jc.AdMob.Avalonia;

public sealed class RewardedInterstitialAd
{
    internal static Func<string?, IRewardedInterstitialAd> ImplementationFactory { get; set; }
    
    static RewardedInterstitialAd()
    {
    }

    public IRewardedInterstitialAd Create(string? unitId = null)
    {
        var implementation = ImplementationFactory(unitId);
        implementation.Load();
        return implementation;
    }
}