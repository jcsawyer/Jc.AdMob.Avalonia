using Android.OS;
using Google.Android.Libraries.Ads.Mobile.Sdk.Common;

namespace Jc.AdMob.Avalonia.Android;

internal static class AdRequest
{
    internal static void ApplyConsent(BaseAdRequestBuilder builder)
    {
        if (!AdMob.Current.Consent.CanShowPersonalizedAds)
        {
            using var extras = new Bundle();
            extras.PutInt("npa", 1);
            builder.SetGoogleExtrasBundle(extras);
        }
    }
}
