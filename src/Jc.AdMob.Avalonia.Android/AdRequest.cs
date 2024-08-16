namespace Jc.AdMob.Avalonia.Android;

internal static class AdRequest
{
    public static global::Android.Gms.Ads.AdRequest GetRequest(Type adapterClass)
    {
        var requestBuilder = new global::Android.Gms.Ads.AdRequest.Builder();
        var requestExtras = new Dictionary<string, string>();
        if (!AdMob.Current.Consent.CanShowPersonalizedAds)
        {
            requestExtras.Add("npa", "1");
        }

        if (requestExtras.Count > 0)
        {
            var extras = new Bundle();
            foreach (var extra in requestExtras)
            {
                extras.PutString(extra.Key, extra.Value);
            }
            requestBuilder.AddNetworkExtrasBundle(Java.Lang.Class.FromType(adapterClass), extras);
        }

        return requestBuilder.Build();
    }
}