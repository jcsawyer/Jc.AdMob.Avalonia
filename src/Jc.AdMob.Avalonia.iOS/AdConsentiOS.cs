using Google.MobileAds;
using Google.UserMessagingPlatform;

namespace Jc.AdMob.Avalonia.iOS;

internal sealed class AdConsentiOS : IAdConsent
{
    private const int GoogleId = 755;
    private readonly AdMobOptions _options;
    private bool _isInitialized;

    public event EventHandler? OnConsentInitialized;
    public event EventHandler<AdError>? OnConsentFailedToInitialize;
    public event EventHandler? OnConsentFormLoaded;
    public event EventHandler<AdError>? OnConsentFormFailedToLoad;
    public event EventHandler<AdError>? OnConsentFormFailedToPresent;
    public event EventHandler? OnConsentProvided;

    public bool IsInitialized => _isInitialized;

    public bool CanShowAds => _options.SkipConsent ||
                              (ConsentInformation.SharedInstance?.ConsentStatus is ConsentStatus.Obtained
                                  or ConsentStatus.NotRequired && CanShowAdsInternal());

    public bool CanShowPersonalizedAds => _options.SkipConsent ||
                                          (ConsentInformation.SharedInstance?.ConsentStatus is ConsentStatus.Obtained
                                              or ConsentStatus.NotRequired && CanShowPersonalizedAdsInternal());

    public AdConsentiOS(AdMobOptions options)
    {
        _options = options;
    }

    public void Initialize()
    {
        if (ConsentInformation.SharedInstance is null)
        {
            OnConsentFailedToInitialize?.Invoke(this,
                new AdError("ConsentInformation instance is null - are you running in an emulator?"));
            return;
        }

        if (_options.SkipConsent ||
            ConsentInformation.SharedInstance.ConsentStatus is ConsentStatus.NotRequired)
        {
            InitializeAds();
            return;
        }

        var requestParameters = new RequestParameters();
        requestParameters.TagForUnderAgeOfConsent = _options.TagForUnderAgeOfConsent;

        requestParameters.DebugSettings = new DebugSettings
        {
            TestDeviceIdentifiers = _options.TestDeviceIds?.ToArray(),
            Geography = _options.DebugGeography switch
            {
                DebugGeography.Eea => Google.UserMessagingPlatform.DebugGeography.Eea,
                DebugGeography.NotEea => Google.UserMessagingPlatform.DebugGeography.NotEea,
                _ => Google.UserMessagingPlatform.DebugGeography.Disabled,
            },
        };

        ConsentInformation.SharedInstance.RequestConsentInfoUpdateWithParameters(requestParameters, error =>
        {
            if (error is not null)
            {
                OnConsentFailedToInitialize?.Invoke(this, new AdError(error.DebugDescription));
                return;
            }

            OnConsentInitialized?.Invoke(this, EventArgs.Empty);
            _isInitialized = true;
        });
    }

    public void Reset()
    {
        if (ConsentInformation.SharedInstance is null)
        {
            return;
        }

        ConsentInformation.SharedInstance.Reset();
        _isInitialized = false;
        Initialize();
    }

    public void ShowConsent()
    {
        if (!_isInitialized)
        {
            OnConsentFormFailedToPresent?.Invoke(this,
                new AdError("Consent is not initialized - try calling Initialize"));
            return;
        }

        var consentStatus = ConsentInformation.SharedInstance.ConsentStatus;
        if (consentStatus is ConsentStatus.Unknown or ConsentStatus.Required)
        {
            ConsentForm.LoadWithCompletionHandler(ConsentFormLoaded);
        }
    }

    private void ConsentFormLoaded(ConsentForm? consentForm, NSError? error)
    {
        if (error is not null || consentForm is null)
        {
            if (error?.Code == 7)
            {
                InitializeAds();
                return;
            }

            OnConsentFormFailedToLoad?.Invoke(this, new AdError(error?.DebugDescription ?? "Consent form was null"));
            return;
        }

        OnConsentFormLoaded?.Invoke(this, EventArgs.Empty);
        consentForm.PresentFromViewController(UIApplication.SharedApplication.KeyWindow.RootViewController,
            ConsentFormPresented);
    }

    private void ConsentFormPresented(NSError? error)
    {
        if (error is not null)
        {
            OnConsentFormFailedToPresent?.Invoke(this, new AdError(error.DebugDescription));
            return;
        }

        InitializeAds();
    }

    private void InitializeAds()
    {
        MobileAds.SharedInstance.Start(_ =>
        {
            if (CanShowAds || CanShowPersonalizedAds)
            {
                OnConsentProvided?.Invoke(this, EventArgs.Empty);
            }

            _isInitialized = true;
            AdMob.Current.AdsInitialized();
        });
    }

    private bool CanShowAdsInternal()
    {
        var userDefaults = NSUserDefaults.StandardUserDefaults;
        var purposeConsent = userDefaults.StringForKey("IABTCF_PurposeConsents");
        var vendorConsent = userDefaults.StringForKey("IABTCF_VendorConsents");
        var vendorLi = userDefaults.StringForKey("IABTCF_VendorLegitimateInterests");
        var purposeLi = userDefaults.StringForKey("IABTCF_PurposeLegitimateInterests");

        var hasGoogleVendorConsent = HasAttribute(vendorConsent, GoogleId);
        var hasGoogleVendorLi = HasAttribute(vendorLi, GoogleId);

        var indexes = new List<int> { 1 };
        var indexesLi = new List<int> { 2, 7, 9, 10 };

        return HasConsentFor(indexes, purposeConsent, hasGoogleVendorConsent) &&
               HasConsentOrLegitimateInterestFor(indexesLi, purposeConsent, purposeLi, hasGoogleVendorConsent,
                   hasGoogleVendorLi);
    }

    private bool CanShowPersonalizedAdsInternal()
    {
        var userDefaults = NSUserDefaults.StandardUserDefaults;
        var purposeConsent = userDefaults.StringForKey("IABTCF_PurposeConsents");
        var vendorConsent = userDefaults.StringForKey("IABTCF_VendorConsents");
        var vendorLi = userDefaults.StringForKey("IABTCF_VendorLegitimateInterests");
        var purposeLi = userDefaults.StringForKey("IABTCF_PurposeLegitimateInterests");

        var hasGoogleVendorConsent = HasAttribute(vendorConsent, GoogleId);
        var hasGoogleVendorLi = HasAttribute(vendorLi, GoogleId);

        var indexes = new List<int> { 1, 3, 4 };
        var indexesLi = new List<int> { 2, 7, 9, 10 };

        return HasConsentFor(indexes, purposeConsent, hasGoogleVendorConsent) &&
               HasConsentOrLegitimateInterestFor(indexesLi, purposeConsent, purposeLi, hasGoogleVendorConsent,
                   hasGoogleVendorLi);
    }

    private bool HasAttribute(string? input, int index)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return input.Length > index && input[index - 1] == '1';
    }

    private bool HasConsentFor(List<int> indexes, string? purposeConsent, bool hasVendorConsent)
    {
        return indexes.TrueForAll(p => HasAttribute(purposeConsent, p)) && hasVendorConsent;
    }

    private bool HasConsentOrLegitimateInterestFor(List<int> indexes, string? purposeConsent, string? purposeLi,
        bool hasVendorConsent,
        bool hasVendorLi)
    {
        return indexes.TrueForAll(p =>
            (HasAttribute(purposeLi, p) && hasVendorLi) || (HasAttribute(purposeConsent, p) && hasVendorConsent));
    }
}