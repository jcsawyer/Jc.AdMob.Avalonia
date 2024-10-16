using Android.Gms.Ads;
using Android.Gms.Ads.Rewarded;

namespace Jc.AdMob.Avalonia.Android;

public sealed class RewardedInterstitialAd : IRewardedInterstitialAd
{
    private const string TestUnit = "ca-app-pub-9127086931863581/2976295622";

    internal static Activity? Activity { get; set; }

    private readonly AdMobOptions? _options;
    private readonly string? _unitId;
    private IReadOnlyCollection<string>? _testDeviceIds;
    private bool _hasLoaded;
    private global::Android.Gms.Ads.RewardedInterstitial.RewardedInterstitialAd? _ad;
    private RewardedInterstitialAdCallbacks? _callbacks;
    
    public event EventHandler? OnAdLoaded;
    public event EventHandler<AdError>? OnAdFailedToLoad;
    public event EventHandler? OnAdPresented;
    public event EventHandler<AdError>? OnAdFailedToPresent;
    public event EventHandler? OnAdImpression;
    public event EventHandler? OnAdClicked;
    public event EventHandler? OnAdClosed;
    public event EventHandler<RewardItem>? OnUserEarnedReward; 

    public RewardedInterstitialAd(string? unitId, IReadOnlyCollection<string>? testDeviceIds = null)
    {
        _unitId = unitId;
        _testDeviceIds = testDeviceIds;
    }

    
    public RewardedInterstitialAd(AdMobOptions options, string? unitId = null)
    {
        _options = options;
        _unitId = unitId;
    }

    public void Load()
    {
        var configBuilder = new RequestConfiguration.Builder();
        if (_testDeviceIds is not null)
        {
            configBuilder.SetTestDeviceIds(_testDeviceIds.ToList());
        }

        if (_options is null || AdMob.Current.CanShowAds)
        {
            var adRequest = _options is null
                ? new global::Android.Gms.Ads.AdRequest.Builder().Build()
                : AdRequest.GetRequest(typeof(RewardedInterstitialAd));

            _callbacks = new RewardedInterstitialAdCallbacks();
            _callbacks.WhenAdLoaded += AdLoaded;
            _callbacks.WhenAdFailedToLoad += (s, e) => OnAdFailedToLoad?.Invoke(s, new AdError(e.Message));
            _callbacks.WhenUserEarnedReward += (s, e) => OnUserEarnedReward?.Invoke(s, new RewardItem(e.Amount, e.Type));
            
            global::Android.Gms.Ads.RewardedInterstitial.RewardedInterstitialAd.Load(Activity,
                string.IsNullOrWhiteSpace(_unitId) ? TestUnit : _unitId, adRequest, _callbacks);
        }
    }

    private void AdLoaded(object? sender, global::Android.Gms.Ads.RewardedInterstitial.RewardedInterstitialAd rewardedInterstitialAd)
    {
        _ad = rewardedInterstitialAd;
        _hasLoaded = true;
        
        OnAdLoaded?.Invoke(this, EventArgs.Empty);
    }

    public void Show()
    {
        if (!_hasLoaded || _ad is null)
        {
            return;
        }
        
        var listener = new InterstitialAdListener();
        
        listener.AdPresented += (s, _) => OnAdPresented?.Invoke(s, EventArgs.Empty);
        listener.AdFailedToPresent += (s, e) => OnAdFailedToPresent?.Invoke(s, new AdError(e.Message));
        listener.AdImpression += (s, _) => OnAdImpression?.Invoke(s, EventArgs.Empty);
        listener.AdClicked += (s, _) => OnAdClicked?.Invoke(s, EventArgs.Empty);
        listener.AdClosed += (s, _) => OnAdClosed?.Invoke(s, EventArgs.Empty);

        _ad.FullScreenContentCallback = listener;
        _ad.Show(Activity, _callbacks);
    }
}