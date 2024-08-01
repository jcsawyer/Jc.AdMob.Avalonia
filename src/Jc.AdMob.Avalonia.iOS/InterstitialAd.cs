using Google.MobileAds;

namespace Jc.AdMob.Avalonia.iOS;

public sealed class InterstitialAd : IInterstitialAd
{
    private const string TestUnit = "ca-app-pub-3940256099942544/4411468910";

    private readonly string? _unitId;
    private bool _hasLoaded;
    private Google.MobileAds.InterstitialAd? _ad;
    
    public event EventHandler? OnAdLoaded;
    public event EventHandler<AdError>? OnAdFailedToLoad;
    public event EventHandler? OnAdPresented;
    public event EventHandler<AdError>? OnAdFailedToPresent;
    public event EventHandler? OnAdImpression;
    public event EventHandler? OnAdClicked;
    public event EventHandler? OnAdClosed;

    public InterstitialAd(string? unitId = null, IReadOnlyCollection<string>? testDeviceIds = null)
    {
        _unitId = unitId;
        if (testDeviceIds is not null)
        {
            MobileAds.SharedInstance.RequestConfiguration.TestDeviceIdentifiers = testDeviceIds.ToArray();
        }
    }

    public void Load()
    {
        var request = Request.GetDefaultRequest();
        
        Google.MobileAds.InterstitialAd.Load(string.IsNullOrWhiteSpace(_unitId) ? TestUnit : _unitId, request, AdLoaded);
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

    private void AdLoaded(Google.MobileAds.InterstitialAd? interstitialAd, NSError? error)
    {
        if (error is not null)
        {
            OnAdFailedToLoad?.Invoke(this, new AdError(error.DebugDescription));
            return;
        }

        if (interstitialAd is null)
        {
            return;
        }
        
        _ad = interstitialAd;

        _ad.PresentedContent += (s, e) => OnAdPresented?.Invoke(s, e);
        _ad.FailedToPresentContent += (s, e) => OnAdFailedToPresent?.Invoke(s, new AdError(e.Error.DebugDescription));
        _ad.RecordedImpression += (s, e) => OnAdImpression?.Invoke(s, e);
        _ad.RecordedClick += (s, e) => OnAdClicked?.Invoke(s, e);
        _ad.DismissedContent += (s, e) => OnAdClosed?.Invoke(s, e);
        
        _hasLoaded = true;
        OnAdLoaded?.Invoke(this, EventArgs.Empty);
    }
}