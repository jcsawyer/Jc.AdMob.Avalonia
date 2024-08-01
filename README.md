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
| Android | ✓ | ✓ | ☓ | ☓ | ☓ | ☓ | ☓ |
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

Then you must register AdMob in the `CustomizeAppBuilder` method in `MainActivity.cs` and `AppDelete.cs` for Android and iOS respectively:

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

Interstitial ads can be used by instantiating `InterstitialAd` in your `App` or DI container and calling `InterstitialAd.Create(unitId)`.

This call will load the return the interstitial ad with an `OnAdLoaded` event once it's finished loaded. `.Show()` can then be called to display the ad.

#### Android

When using interstitial ads on Android, the `Activity` must be passed into the `.AddAdMob()` call:

```c#
protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
{
    return base.CustomizeAppBuilder(builder)
        ...
        .UseAdMob(this);
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