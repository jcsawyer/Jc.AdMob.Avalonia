using Android.Gms.Ads;

namespace Jc.AdMob.Avalonia.Android;

public sealed class InterstitialAd : IInterstitialAd
{
    private const string TestUnit = "ca-app-pub-3940256099942544/1033173712";

    private readonly AdMobOptions? _options;
    private readonly string? _unitId;
    private IReadOnlyCollection<string>? _testDeviceIds;
    private bool _hasLoaded;
    private global::Android.Gms.Ads.Interstitial.InterstitialAd? _ad;

    public event EventHandler? OnAdLoaded;
    public event EventHandler<AdError>? OnAdFailedToLoad;
    public event EventHandler? OnAdPresented;
    public event EventHandler<AdError>? OnAdFailedToPresent;
    public event EventHandler? OnAdImpression;
    public event EventHandler? OnAdClicked;
    public event EventHandler? OnAdClosed;

    internal InterstitialAd(AdMobOptions options, string? unitId = null)
    {
        _options = options;
        _unitId = unitId;
    }

    public void Load()
    {
        var configBuilder = new RequestConfiguration.Builder();
        if (_options?.TestDeviceIds is not null)
        {
            configBuilder.SetTestDeviceIds(_options.TestDeviceIds.ToList());
        }

        MobileAds.RequestConfiguration = configBuilder.Build();

        if (_options is null || AdMob.Current.CanShowAds)
        {
            var adRequest = _options is null
                ? new global::Android.Gms.Ads.AdRequest.Builder().Build()
                : AdRequest.GetRequest(typeof(InterstitialAd));

            var callbacks = new InterstitialAdCallbacks();
            callbacks.WhenAdLoaded += AdLoaded;
            callbacks.WhenAdFailedToLoaded += (s, e) => OnAdFailedToLoad?.Invoke(s, new AdError(e.Message));

            global::Android.Gms.Ads.Interstitial.InterstitialAd.Load(Application.Context,
                string.IsNullOrWhiteSpace(_unitId) ? TestUnit : _unitId, adRequest, callbacks);
        }
        else
        {
            OnAdFailedToLoad?.Invoke(this, new AdError("Consent has not been granted"));
        }
    }

    public void Show()
    {
        if (!_hasLoaded || _ad is null)
        {
            return;
        }

        var activity = ActivityProvider.CurrentActivity;
        if (activity is null)
        {
            OnAdFailedToPresent?.Invoke(this, new AdError("No active Activity is available to present the ad."));
            return;
        }

        var listener = new FullScreenContentCallback();

        listener.AdPresented += (s, _) => OnAdPresented?.Invoke(s, EventArgs.Empty);
        listener.AdFailedToPresent += (s, e) => OnAdFailedToPresent?.Invoke(s, new AdError(e.Message));
        listener.AdImpression += (s, _) => OnAdImpression?.Invoke(s, EventArgs.Empty);
        listener.AdClicked += (s, _) => OnAdClicked?.Invoke(s, EventArgs.Empty);
        listener.AdClosed += (s, _) => OnAdClosed?.Invoke(s, EventArgs.Empty);

        _ad.FullScreenContentCallback = listener;
        _ad.Show(activity);
    }

    private void AdLoaded(object? sender, global::Android.Gms.Ads.Interstitial.InterstitialAd? interstitialAd)
    {
        _ad = interstitialAd;
        _hasLoaded = true;

        OnAdLoaded?.Invoke(this, EventArgs.Empty);
    }
}
