using System;

namespace Jc.AdMob.Avalonia;

public sealed class RewardedAd
{
    internal static Func<string?, IRewardedAd> ImplementationFactory { get; set; }
    
    static RewardedAd()
    {
    }
    
    public IRewardedAd Create(string? unitId = null)
    {
        var implementation = ImplementationFactory(unitId);
        implementation.Load();
        return implementation;
    }
}