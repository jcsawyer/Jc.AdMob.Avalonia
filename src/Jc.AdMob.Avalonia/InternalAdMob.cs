using System;

namespace Jc.AdMob.Avalonia;

internal sealed class InternalAdMob : IAdMob
{
    private AdMobOptions _options;
    private EventHandler? _onAdsInitialized;
    private bool _adsInitialized;
    
    public event EventHandler? OnAdsInitialized
    {
        add
        {
            if (_adsInitialized)
            {
                value?.Invoke(this, EventArgs.Empty);
                return;
            }

            _onAdsInitialized += value;
        }
        remove => _onAdsInitialized -= value;
    }
    
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
        _adsInitialized = true;
        _onAdsInitialized?.Invoke(this, EventArgs.Empty);
    }
}
