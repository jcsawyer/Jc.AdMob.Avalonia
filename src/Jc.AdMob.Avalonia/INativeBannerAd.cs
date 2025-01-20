using System;
using Avalonia.Platform;

namespace Jc.AdMob.Avalonia;

public interface INativeBannerAd
{
    IPlatformHandle? CreateControl(string? unitId, BannerAd wrapper, IPlatformHandle parent, Func<IPlatformHandle> createDefault);
}