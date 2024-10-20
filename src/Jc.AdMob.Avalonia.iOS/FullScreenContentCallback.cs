using Google.MobileAds;

namespace Jc.AdMob.Avalonia.iOS;

internal sealed class FullScreenContentCallback : FullScreenContentDelegate
{
    public event EventHandler? PresentedContent; 
    public event EventHandler<NSError>? FailedToPresentContent;
    public event EventHandler? RecordedImpression;
    public event EventHandler? RecordedClick;
    public event EventHandler? DismissedContent;

    public override void DidPresentFullScreenContent(FullScreenPresentingAd ad)
    {
        PresentedContent?.Invoke(this, EventArgs.Empty);
    }

    public override void DidFailToPresentFullScreenContent(FullScreenPresentingAd ad, NSError error)
    {
        FailedToPresentContent?.Invoke(this, error);
    }

    public override void DidRecordImpression(FullScreenPresentingAd ad)
    {
        RecordedImpression?.Invoke(this, EventArgs.Empty);
    }

    public override void DidRecordClick(FullScreenPresentingAd ad)
    {
        RecordedClick?.Invoke(this, EventArgs.Empty);
    }

    public override void DidDismissFullScreenContent(FullScreenPresentingAd ad)
    {
        DismissedContent?.Invoke(this, EventArgs.Empty);
    }
}