using System;
using System.Threading;

namespace Jc.AdMob.Avalonia;

public static class AdMob
{
    private static readonly Lazy<IAdMob> Implementation =
        new(() => new InternalAdMob(), LazyThreadSafetyMode.PublicationOnly);

    public static IAdMob Current => Implementation.Value;
}