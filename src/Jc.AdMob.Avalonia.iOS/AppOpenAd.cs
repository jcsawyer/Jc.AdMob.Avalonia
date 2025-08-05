using Google.MobileAds;

namespace Jc.AdMob.Avalonia.iOS;

public sealed class AppOpenAd : IAppOpenAd
{
    private const string TestUnit = "ca-app-pub-3940256099942544/5575463023";

    private readonly AdMobOptions? _options;
    private readonly string? _unitId;
    private bool _hasLoaded;
    private Google.MobileAds.AppOpenAd? _ad;
    
    public event EventHandler? OnAdLoaded;
    public event EventHandler<AdError>? OnAdFailedToLoad;
    public event EventHandler? OnAdPresented;
    public event EventHandler<AdError>? OnAdFailedToPresent;
    public event EventHandler? OnAdImpression;
    public event EventHandler? OnAdClicked;
    public event EventHandler? OnAdClosed;

    public AppOpenAd(AdMobOptions options, string? unitId = null)
    {
        _options = options;
        _unitId = unitId;
        if (options.TestDeviceIds is not null)
        {
            MobileAds.SharedInstance.RequestConfiguration.TestDeviceIdentifiers = options.TestDeviceIds.ToArray();
        }
    }
    
    public void Load()
    {
        if (_options is null)
        {
            var request = Request.GetDefaultRequest();
            Google.MobileAds.AppOpenAd.Load(string.IsNullOrWhiteSpace(_unitId) ? TestUnit : _unitId, request, AdLoaded);
            return;
        }

        if (AdMob.Current.CanShowAds)
        {
            var request = AdRequest.GetRequest();
            Google.MobileAds.AppOpenAd.Load(string.IsNullOrWhiteSpace(_unitId) ? TestUnit : _unitId, request, AdLoaded);
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

        var viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
        _ad.Present(viewController);
    }
    
    private void AdLoaded(Google.MobileAds.AppOpenAd? appOpenAd, NSError? error)
    {
        if (error is not null)
        {
            OnAdFailedToLoad?.Invoke(this, new AdError(error.DebugDescription));
            return;
        }

        if (appOpenAd is null)
        {
            return;
        }
        
        _ad = appOpenAd;

        _ad.PresentedContent += (s, e) => OnAdPresented?.Invoke(s, e);
        _ad.FailedToPresentContent += (s, e) => OnAdFailedToPresent?.Invoke(s, new AdError(e.Error.DebugDescription));
        _ad.RecordedImpression += (s, e) => OnAdImpression?.Invoke(s, e);
        _ad.RecordedClick += (s, e) => OnAdClicked?.Invoke(s, e);
        _ad.DismissedContent += (s, e) => OnAdClosed?.Invoke(s, e);
        
        _hasLoaded = true;
        OnAdLoaded?.Invoke(this, EventArgs.Empty);
    }
}