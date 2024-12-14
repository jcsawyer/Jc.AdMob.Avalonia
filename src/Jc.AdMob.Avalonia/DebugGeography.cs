using System;

namespace Jc.AdMob.Avalonia;

public enum DebugGeography
{
    Disabled,
    Eea,
    [Obsolete("Use Other")]
    NotEea,
    RegulatedUsState,
    Other,
}