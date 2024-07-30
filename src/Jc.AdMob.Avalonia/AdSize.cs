namespace Jc.AdMob.Avalonia;

public enum AdSize
{
    /// <summary>
    /// Mobile Marketing Association (MMA) banner ad size (320x50 density-independent pixels).
    /// </summary>
    Banner,
    
    /// <summary>
    /// Interactive Advertising Bureau (IAB) full banner ad size (468x60 density-independent pixels).
    /// </summary>
    FullBanner,
    
    /// <summary>
    /// An invalid AdSize that will cause the ad request to fail immediately.
    /// </summary>
    Invalid,
    
    /// <summary>
    /// Large banner ad size (320x100 density-independent pixels).
    /// </summary>
    LargeBanner,
    
    /// <summary>
    /// Interactive Advertising Bureau (IAB) leaderboard ad size (728x90 density-independent pixels).
    /// </summary>
    Leaderboard,
    
    /// <summary>
    /// Interactive Advertising Bureau (IAB) medium rectangle ad size (300x250 density-independent pixels).
    /// </summary>
    MediumRectangle,
    
    /// <summary>
    ///  IAB wide skyscraper ad size (160x600 density-independent pixels).
    /// </summary>
    WideSkyscraper
}