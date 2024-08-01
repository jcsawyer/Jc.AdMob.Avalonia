using System;

namespace Jc.AdMob.Avalonia;

public sealed class InterstitialAd
{
    internal static Func<string?, IInterstitialAd> ImplementationFactory { get; set; }

    static InterstitialAd()
    {
    }

    public IInterstitialAd Create(string? unitId = null)
    {
        var implementation = ImplementationFactory(unitId);
        implementation.Load();
        return implementation;
    }
}