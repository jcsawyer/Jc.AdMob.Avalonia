using Android.App;
using Android.Gms.Ads;
using Android.Gms.Ads.Initialization;
using Android.Preferences;
using Xamarin.Google.UserMesssagingPlatform;

namespace Jc.AdMob.Avalonia.Android;

internal sealed class AdConsentAndroid : AdLoadCallback, IAdConsent, IConsentInformationOnConsentInfoUpdateSuccessListener, IConsentInformationOnConsentInfoUpdateFailureListener, IConsentFormOnConsentFormDismissedListener, IOnInitializationCompleteListener, UserMessagingPlatform.IOnConsentFormLoadSuccessListener, UserMessagingPlatform.IOnConsentFormLoadFailureListener
{
    private const int GoogleId = 755;
    private readonly AdMobOptions _options;

    public event EventHandler? OnConsentInitialized;
    public event EventHandler<AdError>? OnConsentFailedToInitialize;
    public event EventHandler? OnConsentFormLoaded;
    public event EventHandler<AdError>? OnConsentFormFailedToLoad;
    public event EventHandler<AdError>? OnConsentFormFailedToPresent;
    public event EventHandler? OnConsentFormClosed;
    public event EventHandler? OnConsentProvided;

    private bool _isInitialized;
    private bool _isConsentInfoUpdatePending;

    private IConsentInformation? _consentInformation;

    public bool IsInitialized => _isInitialized;

    public bool CanShowAds => _options.SkipConsent ||
                              _consentInformation?.ConsentStatus is ConsentInformationConsentStatus.NotRequired ||
                              (_consentInformation?.ConsentStatus is ConsentInformationConsentStatus.Obtained && CanShowAdsInternal());

    public bool CanShowPersonalizedAds => _options.SkipConsent ||
                                          _consentInformation?.ConsentStatus is ConsentInformationConsentStatus.NotRequired ||
                                          (_consentInformation?.ConsentStatus is ConsentInformationConsentStatus.Obtained && CanShowPersonalizedAdsInternal());

    public AdConsentAndroid(AdMobOptions options)
    {
        _options = options;
    }

    public void Initialize()
    {
        if (TryGetActivity(out var activity))
        {
            Initialize(activity);
            return;
        }

        if (_isConsentInfoUpdatePending)
        {
            return;
        }

        _isConsentInfoUpdatePending = true;
        ActivityProvider.ActivityChanged += OnActivityChanged;
    }

    private void OnActivityChanged(object? sender, Activity activity)
    {
        if (!_isConsentInfoUpdatePending)
        {
            return;
        }

        _isConsentInfoUpdatePending = false;
        ActivityProvider.ActivityChanged -= OnActivityChanged;
        Initialize(activity);
    }

    private void Initialize(Activity activity)
    {
        try
        {
            var consentInformation = UserMessagingPlatform.GetConsentInformation(activity);
            if (_options.SkipConsent || consentInformation.ConsentStatus is ConsentInformationConsentStatus.NotRequired)
            {
                InitializeAds(activity);
                return;
            }

            var builder = new ConsentDebugSettings.Builder(activity);
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

            consentInformation.RequestConsentInfoUpdate(activity, requestParameters, this, this);
            _consentInformation = consentInformation;
        
        }
        catch (Exception e)
        {
            OnConsentFailedToInitialize?.Invoke(this, new AdError(e.Message));
        }
    }

    public void Reset()
    {
        if (!TryGetActivity(out var activity))
        {
            OnConsentFailedToInitialize?.Invoke(this, new AdError("No active Activity is available for consent reset."));
            return;
        }

        UserMessagingPlatform.GetConsentInformation(activity).Reset();
        Initialize();
    }

    public void ShowConsent()
    {
        if (!TryGetActivity(out var activity))
        {
            OnConsentFormFailedToPresent?.Invoke(this, new AdError("No active Activity is available to show the consent form."));
            return;
        }

        // below method must be called without checking consent requirement,
        // the method will show the form only if the consent is required,
        // and will always fire OnConsentProvided event,
        // which is necessary for ads to be requested at non-GDPR countries,
        // since CanShowAds property is false at CreateControl() during the first launch of app.
        UserMessagingPlatform.LoadAndShowConsentFormIfRequired(activity, this);
        // TODO investigate why this isn't working
        //UserMessagingPlatform.LoadConsentForm(activity, this, this);
    }
    
    public void ShowPrivacyOptions()
    {
        if (!TryGetActivity(out var activity))
        {
            OnConsentFormFailedToPresent?.Invoke(this, new AdError("No active Activity is available to show privacy options."));
            return;
        }

        UserMessagingPlatform.LoadConsentForm(activity, this, this);
    }

    private void InitializeAds(Activity activity)
    {
        OnConsentFormClosed?.Invoke(this, EventArgs.Empty);
        MobileAds.Initialize(activity, this);
    }

    public void OnConsentFormLoadFailure(FormError error)
    {
        OnConsentFormFailedToLoad?.Invoke(this, new AdError(error.Message));
    }

    public void OnConsentFormLoadSuccess(IConsentForm consentForm)
    {
        if (!TryGetActivity(out var activity))
        {
            OnConsentFormFailedToPresent?.Invoke(this, new AdError("No active Activity is available to show privacy options."));
            return;
        }

        OnConsentFormLoaded?.Invoke(this, EventArgs.Empty);
        UserMessagingPlatform.ShowPrivacyOptionsForm(activity, this);
    }

    public void OnConsentFormDismissed(FormError? error)
    {
        if (error is not null)
        {
            OnConsentFormFailedToPresent?.Invoke(this, new AdError($"Consent form dismissed: {error.Message}"));
            return;
        }

        if (!TryGetActivity(out var activity))
        {
            OnConsentFormFailedToPresent?.Invoke(this, new AdError("No active Activity is available to complete consent flow."));
            return;
        }

        _consentInformation = UserMessagingPlatform.GetConsentInformation(activity);
        InitializeAds(activity);
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
        if (!TryGetActivity(out var activity))
        {
            return false;
        }

        var userPreferences = PreferenceManager.GetDefaultSharedPreferences(activity);
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
        if (!TryGetActivity(out var activity))
        {
            return false;
        }

        var userPreferences = PreferenceManager.GetDefaultSharedPreferences(activity);
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

    private static bool TryGetActivity(out Activity activity)
    {
        var currentActivity = ActivityProvider.CurrentActivity;
        if (currentActivity is null)
        {
            activity = null!;
            return false;
        }

        activity = currentActivity;
        return true;
    }
}
