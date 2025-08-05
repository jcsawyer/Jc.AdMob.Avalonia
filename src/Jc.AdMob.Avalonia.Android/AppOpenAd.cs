using Android.Gms.Ads;

namespace Jc.AdMob.Avalonia.Android;

public class AppOpenAd : IAppOpenAd
{
    private const string TestUnit = "ca-app-pub-3940256099942544/9257395921";
    
    internal static Activity? Activity { get; set; }
    
    private readonly AdMobOptions? _options;
    private readonly string? _unitId;
    private bool _hasLoaded;
    private global::Android.Gms.Ads.AppOpen.AppOpenAd? _ad;
    
    public event EventHandler? OnAdLoaded;
    public event EventHandler<AdError>? OnAdFailedToLoad;
    public event EventHandler? OnAdPresented;
    public event EventHandler<AdError>? OnAdFailedToPresent;
    public event EventHandler? OnAdImpression;
    public event EventHandler? OnAdClicked;
    public event EventHandler? OnAdClosed;

    internal AppOpenAd(AdMobOptions options, string? unitId = null)
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
                : AdRequest.GetRequest(typeof(AppOpenAd));

            var callbacks = new AppOpenAdCallbacks();
            callbacks.WhenAdLoaded += AdLoaded;
            callbacks.WhenAdFailedToLoaded += (s, e) => OnAdFailedToLoad?.Invoke(this, new AdError(e.Message));

            global::Android.Gms.Ads.AppOpen.AppOpenAd.Load(Application.Context,
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

        var listener = new FullScreenContentCallback();
        listener.AdPresented += (s, e) => OnAdPresented?.Invoke(this, EventArgs.Empty);
        listener.AdFailedToPresent += (s, e) => OnAdFailedToPresent?.Invoke(this, new AdError(e.Message));
        listener.AdImpression += (s, e) => OnAdImpression?.Invoke(this, EventArgs.Empty);
        listener.AdClicked += (s, e) => OnAdClicked?.Invoke(this, EventArgs.Empty);
        listener.AdClosed += (s, e) => OnAdClosed?.Invoke(this, EventArgs.Empty);
        
        _ad.FullScreenContentCallback = listener;
        _ad.Show(Activity);
    }
    
    private void AdLoaded(object? sender, global::Android.Gms.Ads.AppOpen.AppOpenAd appOpenAd)
    {
        _ad = appOpenAd;
        _hasLoaded = true;
        
        OnAdLoaded?.Invoke(this, EventArgs.Empty);
    }
}