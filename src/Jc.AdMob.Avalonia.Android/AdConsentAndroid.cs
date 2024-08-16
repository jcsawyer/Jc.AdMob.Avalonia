using Android.Gms.Ads;
using Android.Gms.Ads.Initialization;
using Android.Preferences;
using Xamarin.Google.UserMesssagingPlatform;

namespace Jc.AdMob.Avalonia.Android;

internal sealed class AdConsentAndroid : AdLoadCallback, IAdConsent, IConsentInformationOnConsentInfoUpdateSuccessListener, IConsentInformationOnConsentInfoUpdateFailureListener, IConsentFormOnConsentFormDismissedListener, IOnInitializationCompleteListener, UserMessagingPlatform.IOnConsentFormLoadSuccessListener, UserMessagingPlatform.IOnConsentFormLoadFailureListener
{
    private const int GoogleId = 755;
    private readonly AdMobOptions _options;
    private readonly Activity _activity;

    public event EventHandler? OnConsentInitialized;
    public event EventHandler<AdError>? OnConsentFailedToInitialize;
    public event EventHandler? OnConsentFormLoaded;
    public event EventHandler<AdError>? OnConsentFormFailedToLoad;
    public event EventHandler<AdError>? OnConsentFormFailedToPresent;
    public event EventHandler? OnConsentProvided;

    private bool _isInitialized;

    private IConsentInformation _consentInformation;

    public bool IsInitialized => _isInitialized;

    public bool CanShowAds => _options.SkipConsent ||
                              (_consentInformation.ConsentStatus is ConsentInformationConsentStatus.Obtained
                                  or ConsentInformationConsentStatus.NotRequired && CanShowAdsInternal());

    public bool CanShowPersonalizedAds => _options.SkipConsent ||
                                          (_consentInformation.ConsentStatus is ConsentInformationConsentStatus.Obtained
                                               or ConsentInformationConsentStatus.NotRequired &&
                                           CanShowPersonalizedAdsInternal());
    
    public AdConsentAndroid(Activity activity, AdMobOptions options)
    {
        _activity = activity;
        _options = options;
    }

    public void Initialize()
    {
        var consentInformation = UserMessagingPlatform.GetConsentInformation(_activity);
        if (_options.SkipConsent || consentInformation.ConsentStatus is ConsentInformationConsentStatus.NotRequired)
        {
            InitializeAds();
            return;
        }

        var builder = new ConsentDebugSettings.Builder(_activity);
        if (_options.TestDeviceIds is not null)
        {
            foreach (var testDeviceId in _options.TestDeviceIds)
            {
                builder.AddTestDeviceHashedId(testDeviceId);
            }
        }

        if (_options.DebugGeography is not DebugGeography.Disabled)
        {
            builder.SetDebugGeography((int)_options.DebugGeography);
        }

        var consentDebugSettings = builder.Build();
        var requestParameters = new ConsentRequestParameters.Builder()
            .SetConsentDebugSettings(consentDebugSettings)
            .SetTagForUnderAgeOfConsent(_options.TagForUnderAgeOfConsent)
            .SetAdMobAppId(_options.AppId)
            .Build();

        consentInformation.RequestConsentInfoUpdate(_activity, requestParameters, this, this);
        _consentInformation = consentInformation;
    }

    public void Reset()
    {
        UserMessagingPlatform.GetConsentInformation(_activity).Reset();
        Initialize();
    }

    public void ShowConsent()
    {
        if (_consentInformation.PrivacyOptionsRequirementStatus ==
            ConsentInformationPrivacyOptionsRequirementStatus.Required)
        {
            UserMessagingPlatform.ShowPrivacyOptionsForm(_activity, this);
        }
    }

    private void InitializeAds()
    {
        MobileAds.Initialize(_activity, this);
    }

    public void OnConsentFormLoadFailure(FormError error)
    {
        OnConsentFormFailedToLoad?.Invoke(this, new AdError(error.Message));
    }

    public void OnConsentFormLoadSuccess(IConsentForm consentForm)
    {
        OnConsentFormLoaded?.Invoke(this, EventArgs.Empty);
    }

    public void OnConsentFormDismissed(FormError? error)
    {
        if (error is not null)
        {
            OnConsentFormFailedToPresent?.Invoke(this, new AdError($"Consent form dismissed: {error.Message}"));
            return;
        }

        _consentInformation = UserMessagingPlatform.GetConsentInformation(_activity);
        InitializeAds();
    }

    public void OnInitializationComplete(IInitializationStatus status)
    {
        if (CanShowAds || CanShowPersonalizedAds)
        {
            OnConsentProvided?.Invoke(this, EventArgs.Empty);
        }

        _isInitialized = true;
        AdMob.Current.AdsInitialized();
    }

    public void OnConsentInfoUpdateFailure(FormError error)
    {
        OnConsentFailedToInitialize?.Invoke(this, new AdError(error.Message));
    }

    public void OnConsentInfoUpdateSuccess()
    {
        OnConsentInitialized?.Invoke(this, EventArgs.Empty);
        _isInitialized = true;
    }

    private bool CanShowAdsInternal()
    {
        var userPreferences = PreferenceManager.GetDefaultSharedPreferences(_activity);
        var purposeConsent = userPreferences.GetString("IABTCF_PurposeConsents", "");
        var vendorConsent = userPreferences.GetString("IABTCF_VendorConsents", "");
        var vendorLi = userPreferences.GetString("IABTCF_VendorLegitimateInterests", "");
        var purposeLi = userPreferences.GetString("IABTCF_PurposeLegitimateInterests", "");

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
        var userPreferences = PreferenceManager.GetDefaultSharedPreferences(_activity);
        var purposeConsent = userPreferences.GetString("IABTCF_PurposeConsents", "");
        var vendorConsent = userPreferences.GetString("IABTCF_VendorConsents", "");
        var vendorLi = userPreferences.GetString("IABTCF_VendorLegitimateInterests", "");
        var purposeLi = userPreferences.GetString("IABTCF_PurposeLegitimateInterests", "");

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