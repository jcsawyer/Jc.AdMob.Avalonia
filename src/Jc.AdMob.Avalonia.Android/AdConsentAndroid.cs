using Android.App;
using Android.Content.PM;
using Android.Preferences;
using Avalonia.Threading;
using Google.Android.Libraries.Ads.Mobile.Sdk;
using Google.Android.Libraries.Ads.Mobile.Sdk.Common;
using Google.Android.Libraries.Ads.Mobile.Sdk.Initialization;
using Xamarin.Google.UserMesssagingPlatform;

namespace Jc.AdMob.Avalonia.Android;

internal sealed class AdConsentAndroid : Java.Lang.Object, IAdConsent,
    IConsentInformationOnConsentInfoUpdateSuccessListener, IConsentInformationOnConsentInfoUpdateFailureListener,
    IConsentFormOnConsentFormDismissedListener, IOnAdapterInitializationCompleteListener
{
    private const int GoogleId = 755;
    private const string AppIdMetadata = "com.google.android.gms.ads.APPLICATION_ID";
    private readonly TaskCompletionSource<AdError?> _sdkInitialization =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly AdMobOptions _options;
    private readonly HashSet<string> _testDeviceIds;
    private EventHandler? _onConsentInitialized;
    private bool _sdkInitializationStarted;
    private string? _appId;

    internal Task<AdError?> SdkInitialization => _sdkInitialization.Task;
    internal event Action? EligibilityChanged;

    public event EventHandler? OnConsentInitialized
    {
        add
        {
            if (_isInitialized)
            {
                value?.Invoke(this, EventArgs.Empty);
                return;
            }

            _onConsentInitialized += value;
        }
        remove => _onConsentInitialized -= value;
    }
    public event EventHandler<AdError>? OnConsentFailedToInitialize;
    // UMP's combined show APIs report errors through the dismissal listener.
    public event EventHandler? OnConsentFormLoaded { add { } remove { } }
    public event EventHandler<AdError>? OnConsentFormFailedToLoad { add { } remove { } }
    public event EventHandler<AdError>? OnConsentFormFailedToPresent;
    public event EventHandler? OnConsentFormClosed;
    public event EventHandler? OnConsentProvided;

    private bool _isInitialized;
    private bool _isConsentInfoUpdatePending;
    private bool _consentFormPending;

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
        _testDeviceIds = options.TestDeviceIds?.ToHashSet() ?? [];
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
            _appId ??= ResolveAppId(activity);
            if (string.IsNullOrWhiteSpace(_appId))
            {
                FailSdkInitialization(new AdError("Set AdMobOptions.AppId or the AdMob application ID in AndroidManifest.xml."));
                return;
            }

            if (_options.SkipConsent)
            {
                OnConsentInfoUpdateSuccess();
                return;
            }

            var consentInformation = UserMessagingPlatform.GetConsentInformation(activity);
            _consentInformation = consentInformation;
            if (consentInformation.ConsentStatus is ConsentInformationConsentStatus.NotRequired)
            {
                OnConsentInfoUpdateSuccess();
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
                .SetAdMobAppId(_appId)
                .Build();

            consentInformation.RequestConsentInfoUpdate(activity, requestParameters, this, this);
        }
        catch (Exception e)
        {
            var error = new AdError(e.Message);
            if (_appId is null)
            {
                FailSdkInitialization(error);
                return;
            }

            OnConsentFailedToInitialize?.Invoke(this, error);
            InitializeAds();
        }
    }

    public void Reset()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(Reset);
            return;
        }

        if (_consentFormPending)
        {
            return;
        }

        if (!TryGetActivity(out var activity))
        {
            OnConsentFailedToInitialize?.Invoke(this, new AdError("No active Activity is available for consent reset."));
            return;
        }

        _isInitialized = false;
        UserMessagingPlatform.GetConsentInformation(activity).Reset();
        EligibilityChanged?.Invoke();
        Initialize();
    }

    public void ShowConsent() => ShowForm(privacyOptions: false);

    public void ShowPrivacyOptions() => ShowForm(privacyOptions: true);

    private void ShowForm(bool privacyOptions)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => ShowForm(privacyOptions));
            return;
        }

        if (_consentFormPending)
        {
            return;
        }

        if (!TryGetActivity(out var activity))
        {
            OnConsentFormFailedToPresent?.Invoke(this, new AdError("No active Activity is available to show the consent form."));
            return;
        }

        _consentFormPending = true;
        try
        {
            if (privacyOptions)
            {
                UserMessagingPlatform.ShowPrivacyOptionsForm(activity, this);
            }
            else
            {
                UserMessagingPlatform.LoadAndShowConsentFormIfRequired(activity, this);
            }
        }
        catch (Exception error)
        {
            _consentFormPending = false;
            OnConsentFormFailedToPresent?.Invoke(this, new AdError(error.Message));
        }
    }

    private string? ResolveAppId(Activity activity)
    {
        if (!string.IsNullOrWhiteSpace(_options.AppId))
        {
            return _options.AppId.Trim();
        }

        return activity.PackageManager?.GetApplicationInfo(activity.PackageName!, PackageInfoFlags.MetaData)
            ?.MetaData?.GetString(AppIdMetadata)?.Trim();
    }

    private void InitializeAds()
    {
        if (!AdMob.Current.CanShowAds)
        {
            return;
        }

        if (_sdkInitialization.Task.IsCompletedSuccessfully)
        {
            if (_sdkInitialization.Task.Result is null)
            {
                OnConsentProvided?.Invoke(this, EventArgs.Empty);
            }

            return;
        }

        if (_sdkInitializationStarted)
        {
            return;
        }

        _sdkInitializationStarted = true;
        var initialTestDeviceIds = _testDeviceIds.ToList();
        _ = Task.Run(() =>
        {
            try
            {
                var configuration = new InitializationConfig.Builder(_appId!)
                    .SetRequestConfiguration(BuildRequestConfiguration(initialTestDeviceIds))
                    .Build();
                MobileAds.Initialize(Application.Context!, configuration, this);
            }
            catch (Exception error)
            {
                Dispatcher.UIThread.Post(() => FailSdkInitialization(new AdError(error.Message)));
            }
        });
    }

#pragma warning disable CS0618 // Preserve the existing TFCD and TFUA options.
    private RequestConfiguration BuildRequestConfiguration(IList<string> testDeviceIds)
    {
        using var builder = new RequestConfiguration.Builder();
        builder.SetTestDeviceIds(testDeviceIds);
        if (_options.TagForChildDirectedTreatment)
        {
            builder.SetTagForChildDirectedTreatment(
                RequestConfiguration.TagForChildDirectedTreatment.TagForChildDirectedTreatmentTrue!);
        }

        if (_options.TagForUnderAgeOfConsent)
        {
            builder.SetTagForUnderAgeOfConsent(
                RequestConfiguration.TagForUnderAgeOfConsent.TagForUnderAgeOfConsentTrue!);
        }

        return builder.Build();
    }

#pragma warning restore CS0618

    internal void AddTestDevices(IReadOnlyCollection<string>? testDeviceIds)
    {
        if (testDeviceIds is null || testDeviceIds.All(_testDeviceIds.Contains))
        {
            return;
        }

        _testDeviceIds.UnionWith(testDeviceIds);
        MobileAds.RequestConfiguration = BuildRequestConfiguration(_testDeviceIds.ToList());
    }

    private void FailSdkInitialization(AdError error)
    {
        if (_sdkInitialization.TrySetResult(error))
        {
            OnConsentFailedToInitialize?.Invoke(this, error);
        }
    }

    public void OnConsentFormDismissed(FormError? error)
    {
        _consentFormPending = false;
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
        EligibilityChanged?.Invoke();
        OnConsentFormClosed?.Invoke(this, EventArgs.Empty);
        InitializeAds();
    }

    public void OnAdapterInitializationComplete(IInitializationStatus status) => Dispatcher.UIThread.Post(() =>
    {
        if (!_sdkInitialization.TrySetResult(null))
        {
            return;
        }

        AdMob.Current.AdsInitialized();
        if (AdMob.Current.CanShowAds)
        {
            OnConsentProvided?.Invoke(this, EventArgs.Empty);
        }
    });

    public void OnConsentInfoUpdateFailure(FormError error)
    {
        OnConsentFailedToInitialize?.Invoke(this, new AdError(error.Message));
        InitializeAds();
    }

    public void OnConsentInfoUpdateSuccess()
    {
        _isInitialized = true;
        EligibilityChanged?.Invoke();
        _onConsentInitialized?.Invoke(this, EventArgs.Empty);
        InitializeAds();
    }

    private bool CanShowAdsInternal()
    {
        if (!TryGetActivity(out var activity))
        {
            return false;
        }

        var userPreferences = PreferenceManager.GetDefaultSharedPreferences(activity);
        var purposeConsent = userPreferences?.GetString("IABTCF_PurposeConsents", "");
        var vendorConsent = userPreferences?.GetString("IABTCF_VendorConsents", "");
        var vendorLi = userPreferences?.GetString("IABTCF_VendorLegitimateInterests", "");
        var purposeLi = userPreferences?.GetString("IABTCF_PurposeLegitimateInterests", "");

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
        var purposeConsent = userPreferences?.GetString("IABTCF_PurposeConsents", "");
        var vendorConsent = userPreferences?.GetString("IABTCF_VendorConsents", "");
        var vendorLi = userPreferences?.GetString("IABTCF_VendorLegitimateInterests", "");
        var purposeLi = userPreferences?.GetString("IABTCF_PurposeLegitimateInterests", "");

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

        return input.Length >= index && input[index - 1] == '1';
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
