using System;

namespace Jc.AdMob.Avalonia;

public interface IAdConsent
{
    event EventHandler? OnConsentInitialized;
    event EventHandler<AdError>? OnConsentFailedToInitialize;
    event EventHandler? OnConsentFormLoaded;
    event EventHandler<AdError>? OnConsentFormFailedToLoad;
    event EventHandler<AdError>? OnConsentFormFailedToPresent;
    event EventHandler? OnConsentProvided;
    
    bool IsInitialized { get; }
    bool CanShowAds { get; }
    bool CanShowPersonalizedAds { get; }
    
    void Initialize();
    void Reset();
    void ShowConsent();
}