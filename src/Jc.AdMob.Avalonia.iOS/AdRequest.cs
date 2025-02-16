using Google.MobileAds;

namespace Jc.AdMob.Avalonia.iOS;

internal static class AdRequest
{
    public static Request GetRequest()
    {
        MobileAds.SharedInstance.RequestConfiguration.TagForChildDirectedTreatment = AdMob.Current.Options.TagForChildDirectedTreatment;
        MobileAds.SharedInstance.RequestConfiguration.TagForUnderAgeOfConsent = AdMob.Current.Options.TagForUnderAgeOfConsent;
        if (AdMob.Current.Options.TestDeviceIds is not null)
        {
            MobileAds.SharedInstance.RequestConfiguration.TestDeviceIdentifiers = AdMob.Current.Options.TestDeviceIds.ToArray();
        }

        var defaultRequest = Request.GetDefaultRequest();
        
        var requestExtras = new Dictionary<string, string>();
        if (!AdMob.Current.Consent.CanShowPersonalizedAds)
        {
            requestExtras.Add("npa", "1");
        }

        if (requestExtras.Count > 0)
        {
            var extras = new Extras();
            extras.AdditionalParameters = NSDictionary.FromObjectsAndKeys(requestExtras.Values.Cast<object>().ToArray(), requestExtras.Keys.Cast<object>().ToArray());
            defaultRequest.RegisterAdNetworkExtras(extras);
        }

        return defaultRequest;
    }
}