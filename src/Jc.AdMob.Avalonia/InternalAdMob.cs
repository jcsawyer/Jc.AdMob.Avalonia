using System;

namespace Jc.AdMob.Avalonia;

internal sealed class InternalAdMob : IAdMob
{
    private AdMobOptions _options;
    
    public event EventHandler? OnAdsInitialized;
    
    public AppOpenAd AppOpen { get; set; }
    
    public IAdConsent Consent { get; set; }
    
    public InterstitialAd Interstitial { get; set; }

    public RewardedInterstitialAd RewardedInterstitial { get; set; }
    
    public RewardedAd Rewarded { get; set; }

    public AdMobOptions Options => _options;

    public bool CanShowAds => Consent.CanShowAds || Consent.CanShowPersonalizedAds;

    public void Initialize(AdMobOptions options)
    {
        _options = options;
        Consent.Initialize();
    }

    public void AdsInitialized()
    {
        OnAdsInitialized?.Invoke(this, EventArgs.Empty);
    }
}