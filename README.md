<h1 align="center">
    <br>
    Jc.AdMob.Avalonia
    <br>
    &nbsp;
</h1>
<h4 align="center">
    Library to bring AdMob advertisements to Avalonia mobile projects.
</h4>
<h6 align="center">
    Avalonia solution derived from <a href="https://github.com/marius-bughiu/Plugin.AdMob/tree/main">marius-bughiu/Plugin.AdMob</a>
</h6>
<hr>

## Table of Contents
- [Introduction](#introduction)
- [Usage](#usage)

## Introduction

Jc.AdMob.Avalonia is a library to bring [Google AdMob](https://developers.google.com/admob) services to Avalonia Android and iOS projects.

The library currently supports the following ad units:

| | Banner | Interstitial | Rewarded interstitial | Rewarded | Native advanced | App open |
|---|---|---|---|---|---|---|
| Android | ✓ | ✓ | ✓ | ☓ | ☓ | ☓ | ☓ |
| iOS | ✓ | ✓ | ☓ | ☓ | ☓ | ☓ | ☓ |

## Usage

To use Jc.AdMob.Avalonia you must add the `Jc.AdMob.Avalonia` package to your cross-platform project.

```
dotnet add package Jc.AdMob.Avalonia
```

Followed by adding the Android/iOS `Jc.AdMob.Avalonia.xxx` package to the platform specific project(s).

```
dotnet add package Jc.AdMob.Avalonia.Android
dotnet add package Jc.AdMob.Avalonia.iOS
```

Then you must register AdMob in the `CustomizeAppBuilder` method in `MainActivity.cs` and `AppDelegate.cs` for Android and iOS respectively:

```c#
protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
{
    return base.CustomizeAppBuilder(builder)
        ...
        .UseAdMob();
}
```

Finally, follow the AdMob platform specific instructions in regard to configuring the application/unit ids.

### Test Devices

To configure test devices, the `.UseAdMob()` method accepts a collection of test device ids.

### Consent
Google requires all publishers serving ads to EEA and UK users to use a Google-Certified Consent Management Platform (CMP).

Jc.AdMob.Avalonia provides a consent service using the Google User Messaging Platform (UMP) SDK.

> The addition of consent makes the previous `.UseAdMob(testDeviceIds)` obsolete and will be removed in the next major version bump.

#### Usage
To use the consent flow, you must call `.UseAdMob(AdMobOptions)` from the `MainActivity.cs` and/or `AppDelegate.cs`:

```c#
protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
{
    return base.CustomizeAppBuilder(builder)
        ...
        .UseAdMob(new AdMobOptions
        {
            TestDeviceIds = [],
            TagForUnderAgeOfConsent = false,
            TagForChildDirectedTreatment = false,
        });
}
```

When using the consent service, all services are accesed via the singleton `AdMob.Current`.

To enable ads in regions where consent is required, you must show the consent dialog once consent is initialized:

```c#
AdMob.Current.Consent.OnConsentInitialized += (_, _) => AdMob.Current.Consent.ShowConsent();
```

#### Events
| Event | Notes |
|---|---|
| OnConsentInitialized | |
| OnConsentFailedToInitialize | |
| OnConsentFormLoaded | iOS only - issue being investigated |
| OnConsentFormFailedToLoad | iOS only - issue being investigated |
| OnConsentFormFailedToPresent | |
| OnConsentProvided | User consented and ads can be shown |

### Banners

| iOS                               | Android |
|-----------------------------------|---|
| <img alt="iOS Banner" src="img/iOS Banner.png" width="250" /> | <img alt="iOS Banner" src="img/Android Banner.jpeg" width="250" /> |


The `BannerAd` control can be added to your XAML like so:

```xaml
...
xmlns:admob="https://github.com/jc-admob-avalonia"
...

<admob:BannerAd UnitId="{ADMOB_UNIT_ID}" />
```

#### Sizes
The banner can be configured with the following sizes:

| AdSize | Size |
|---|---|
| Banner | Mobile Marketing Association (MMA) banner ad size (320x50 density-independent pixels). |
| FullBanner | Interactive Advertising Bureau (IAB) full banner ad size (468x60 density-independent pixels). |
| Invalid | An invalid AdSize that will cause the ad request to fail immediately. |
| LargeBanner | Large banner ad size (320x100 density-independent pixels). |
| Leaderboard | Interactive Advertising Bureau (IAB) leaderboard ad size (728x90 density-independent pixels). |
| MediumRectangle | Interactive Advertising Bureau (IAB) medium rectangle ad size (300x250 density-independent pixels). |
| WideSkyscrapper | IAB wide skyscraper ad size (160x600 density-independent pixels). |

> Note: If the size is left unspecified, the banner will resize to fit the parent container.

#### Events

The banner exposes the following events:

| Event | Notes |
|---|---|
| OnAdLoaded | |
| OnAdFailedToLoad | |
| OnAdImpression | |
| OnAdClicked | |
| OnAdOpened | |
| OnAdClosed | |
| OnAdSwiped | Android only |

### Interstitial

| iOS                               | Android |
|-----------------------------------|---|
| <img alt="iOS Banner" src="img/iOS Interstitial.png" width="250" /> | <img alt="iOS Banner" src="img/Android Interstitial.jpeg" width="250" /> |

Interstitial ads can be used by calling `InterstitialAd.Create(unitId)` on the `AdMob` singleton.

This call will load and the return the interstitial ad with an `OnAdLoaded` event once it's finished loaded. `.Show()` can then be called to display the ad.

```c#
public ICommand ShowInterstitialAdCommand { get; }

public MainViewModel()
{
    ShowInterstitialAdCommand = ReactiveCommand.Create(ShowInterstitialAd);
}

private void ShowInterstitialAd()
{
    var interstitial = AdMob.Current.Interstitial.Create();
    interstitial.OnAdLoaded += (_, _) => interstitial.Show();
}
```

#### Android

When using interstitial ads on Android, the `Activity` must be passed into the `.AddAdMob()` call:

```c#
protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
{
    return base.CustomizeAppBuilder(builder)
        ...
        .UseAdMob(this, adMobOptions);
}
```

#### Events

| Event | Notes |
|---|---|
| OnAdLoaded | |
| OnAdFailedToLoad | |
| OnAdPresented | |
| OnAdFailedToPresent | |
| OnAdImpression | |
| OnAdClicked | |
| OnAdClosed | |


### Rewarded Interstitial

| iOS                               | Android |
|-----------------------------------|---|
|  | <img alt="iOS Banner" src="img/Android Rewarded Interstitial.jpg" width="250" /> |

Interstitial ads can be used by calling `InterstitialAd.Create(unitId)` on the `AdMob` singleton.

This call will load and the return the interstitial ad with an `OnAdLoaded` event once it's finished loaded. `.Show()` can then be called to display the ad.

```c#
public ICommand ShowRewardedInterstitialAdCommand { get; }

public MainViewModel()
{
    ShowRewardedInterstitialAdCommand = ReactiveCommand.Create(ShowRewardedInterstitialAd);
}

private void ShowRewardedInterstitialAd()
{
    var rewardedInterstitial = AdMob.Current.RewardedInterstitial.Create();
    rewardedInterstitial.OnAdLoaded += (_, _) => rewardedInterstitial.Show();
    rewardedInterstitial.OnUserEarnedReward += (_, reward) => Debug.WriteLine($"User earned reward: {reward.Amount} {reward.Type}");
}
```

#### Android

When using interstitial ads on Android, the `Activity` must be passed into the `.AddAdMob()` call:

```c#
protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
{
    return base.CustomizeAppBuilder(builder)
        ...
        .UseAdMob(this, adMobOptions);
}
```

#### Events

| Event | Notes |
|---|---|
| OnAdLoaded | |
| OnAdFailedToLoad | |
| OnAdPresented | |
| OnAdFailedToPresent | |
| OnAdImpression | |
| OnAdClicked | |
| OnAdClosed | |
| OnUserEarnedReward | Contains a `RewardItem` record |

#### RewardItem

The reward item record is comprised of the following properties:

| Property | Description |
| --- | --- |
| Amount | The amount rewarded. |
| Type | The type of reward earned. |
