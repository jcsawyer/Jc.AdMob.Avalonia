using System.Collections.Generic;

namespace Jc.AdMob.Avalonia;

public sealed record AdMobOptions
{
    public IReadOnlyCollection<string>? TestDeviceIds { get; init; }
    public bool SkipConsent { get; init; }
    public bool TagForChildDirectedTreatment { get; init; }
    public bool TagForUnderAgeOfConsent { get; init; }
    public DebugGeography DebugGeography { get; init; }
    public string AppId { get; init; }
}