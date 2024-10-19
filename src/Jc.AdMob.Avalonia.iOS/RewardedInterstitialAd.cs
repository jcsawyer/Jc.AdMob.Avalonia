using Google.MobileAds;

namespace Jc.AdMob.Avalonia.iOS;

public sealed class RewardedInterstitialAd : IRewardedInterstitialAd
{
    private const string TestUnit = "ca-app-pub-9127086931863581/4486932195";

    private readonly AdMobOptions? _options;
    private readonly string? _unitId;
    private bool _hasLoaded;
    private Google.MobileAds.RewardedInterstitialAd? _ad;

    public event EventHandler? OnAdLoaded;
    public event EventHandler<AdError>? OnAdFailedToLoad;
    public event EventHandler? OnAdPresented;
    public event EventHandler<AdError>? OnAdFailedToPresent;
    public event EventHandler? OnAdImpression;
    public event EventHandler? OnAdClicked;
    public event EventHandler? OnAdClosed;
    public event EventHandler<RewardItem>? OnUserEarnedReward;
    
    public RewardedInterstitialAd(string? unitId = null, IReadOnlyCollection<string>? testDeviceIds = null)
    {
        _unitId = unitId;
        if (testDeviceIds is not null)
        {
            MobileAds.SharedInstance.RequestConfiguration.TestDeviceIdentifiers = testDeviceIds.ToArray();
        }
    }
    
    internal RewardedInterstitialAd(AdMobOptions options, string? unitId = null)
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
            Google.MobileAds.RewardedInterstitialAd.Load(string.IsNullOrWhiteSpace(_unitId) ? TestUnit : _unitId, request, AdLoaded);
            return;
        }

        if (AdMob.Current.CanShowAds)
        {
            var request = AdRequest.GetRequest();
            Google.MobileAds.RewardedInterstitialAd.Load(string.IsNullOrWhiteSpace(_unitId) ? TestUnit : _unitId, request, AdLoaded);
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

        _ad.Present(viewController, () => OnUserEarnedReward?.Invoke(this, new RewardItem(_ad.Reward.Amount.Int32Value, _ad.Reward.Type)));
    }

    private void AdLoaded(Google.MobileAds.RewardedInterstitialAd? rewardedInterstitialAd, NSError? error)
    {
        if (error is not null)
        {
            OnAdFailedToLoad?.Invoke(this, new AdError(error.DebugDescription));
            return;
        }

        if (rewardedInterstitialAd is null)
        {
            return;
        }
        
        _ad = rewardedInterstitialAd;

        var callbacks = new RewardedInterstitialAdCallbacks();
        callbacks.PresentedContent += (s, e) => OnAdPresented?.Invoke(s, e);
        callbacks.FailedToPresentContent += (s, e) => OnAdFailedToPresent?.Invoke(s, new AdError(e.DebugDescription));
        callbacks.RecordedImpression += (s, e) => OnAdImpression?.Invoke(s, e);
        callbacks.RecordedClick += (s, e) => OnAdClicked?.Invoke(s, e);
        callbacks.DismissedContent += (s, e) => OnAdClosed?.Invoke(s, e);
        _ad.FullScreenContentDelegate = callbacks;

        _hasLoaded = true;
        OnAdLoaded?.Invoke(this, EventArgs.Empty);
    }
}