using System;

namespace Jc.AdMob.Avalonia;

public interface IAdMob
{
    public event EventHandler? OnAdsInitialized;
    
    IAdConsent Consent { get; internal set; }
    InterstitialAd Interstitial { get; internal set; }
    
    AdMobOptions Options { get; }
    
    bool CanShowAds { get; }
    
    void Initialize(AdMobOptions options);
    internal void AdsInitialized();
}