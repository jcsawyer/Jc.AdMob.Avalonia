using System;

namespace Jc.AdMob.Avalonia;

public sealed class AppOpenAd
{
    internal static Func<string?, IAppOpenAd> ImplementationFactory { get; set; }
    
    static AppOpenAd()
    {
    }
    
    public IAppOpenAd Create(string? unitId = null)
    {
        var implementation = ImplementationFactory(unitId);
        implementation.Load();
        return implementation;
    }
}