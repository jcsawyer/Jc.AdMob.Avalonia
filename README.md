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

| | Consent | Banner | Interstitial | Rewarded interstitial | Rewarded | Native advanced | App open |
|---|---|---|---|---|---|---|---|
| Android | ✓ | ✓ | ✓ | ✓ | ✓ | ☓ | ☓ | ☓ |
| iOS | ✓ | ✓ | ✓ | ✓ | ✓ | ☓ | ☓ | ☓ |

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
        .UseAdMob(new AdMobOptions
        {
            TestDeviceIds = [],
            TagForUnderAgeOfConsent = false,
            TagForChildDirectedTreatment = false,
        });
}
```

Finally, follow the AdMob platform specific instructions in regard to configuring the application/unit ids.

### Test Devices

To configure test devices, the `.UseAdMob()` method accepts a collection of test device ids.

### Consent
Google requires all publishers serving ads to EEA and UK users to use a Google-Certified Consent Management Platform (CMP).

Jc.AdMob.Avalonia provides a consent service using the Google User Messaging Platform (UMP) SDK.

#### Usage
When using the consent service, all services are accessed via the singleton `AdMob.Current`.

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

Interstitial ads can be used by calling `Interstitial.Create(unitId)` on the `AdMob` singleton.

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
| <img alt="iOS Banner" src="img/iOS Rewarded Interstitial.jpeg" width="250" /> | <img alt="iOS Banner" src="img/Android Rewarded Interstitial.jpg" width="250" /> |

Interstitial ads can be used by calling `RewardedInterstitial.Create(unitId)` on the `AdMob` singleton.

This call will load and the return the interstitial ad with an `OnAdLoaded` event once it's finished loaded. `.Show()` can then be called to display the ad.

The reward event `OnUserEarnedReward` can then be used to know when a reward can be given to the user.

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

### Rewarded

| iOS                               | Android |
|-----------------------------------|---|
| <img alt="iOS Banner" src="img/iOS Rewarded Interstitial.jpeg" width="250" /> | <img alt="iOS Banner" src="img/Android Rewarded Interstitial.jpg" width="250" /> |

Interstitial ads can be used by calling `Rewarded.Create(unitId)` on the `AdMob` singleton.

This call will load and the return the interstitial ad with an `OnAdLoaded` event once it's finished loaded. `.Show()` can then be called to display the ad.

The reward event `OnUserEarnedReward` can then be used to know when a reward can be given to the user.

```c#
public ICommand ShowRewardedAdCommand { get; }

public MainViewModel()
{
    ShowRewardedAdCommand = ReactiveCommand.Create(ShowRewardedAd);
}

private void ShowRewardedAd()
{
    var rewarded = AdMob.Current.Rewarded.Create();
    rewarded.OnAdLoaded += (_, _) => rewarded.Show();
    rewarded.OnUserEarnedReward += (_, reward) => Debug.WriteLine($"User earned reward: {reward.Amount} {reward.Type}");
}
```

#### Android

When using rewarded ads on Android, the `Activity` must be passed into the `.AddAdMob()` call:

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

## Troubleshooting

### The Google Mobile Ads SDK was initialized without AppMeasurement
If you receive an error similar to the following:

```
GADInvalidInitializationException Reason: The Google Mobile Ads SDK was initialized without AppMeasurement. Google AdMob publishers, follow instructions here: https://googlemobileadssdk.page.link/admob-ios-update-plist to include the AppMeasurement framework and set the -ObjC linker flag. Google Ad Manager publishers, follow instructions here: https://googlemobileadssdk.page.link/ad-manager-ios-update-plist
```

Try adding the following to your `Info.plist`:

```xml
<key>GADIsAdManagerApp</key>
<true/>
```

> The current iOS bindings are a little out of date and are still using an older version of the Google Mobile Ads SDK. While the version is now no longer a sunset version by Google, this is actively being worked on [here](https://github.com/jcsawyer/Jc.GoogleMobileAds.iOS).