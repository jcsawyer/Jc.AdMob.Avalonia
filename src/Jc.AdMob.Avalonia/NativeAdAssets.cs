using System;
using Avalonia;
using Avalonia.Controls;

namespace Jc.AdMob.Avalonia;

public abstract class NativeAdAsset : Control
{
    private protected NativeAdAsset() { }

    internal Func<Size, Size>? MeasureAsset { get; set; }

    protected override Size MeasureOverride(Size availableSize) => MeasureAsset?.Invoke(availableSize) ?? default;
}

public sealed class NativeAdAttribution : NativeAdAsset
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<NativeAdAttribution, string>(nameof(Text), "Ad");

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}

public sealed class NativeAdHeadline : NativeAdAsset;
public sealed class NativeAdIcon : NativeAdAsset;
public sealed class NativeAdMedia : NativeAdAsset
{
    static NativeAdMedia()
    {
        MinWidthProperty.OverrideDefaultValue<NativeAdMedia>(120);
        MinHeightProperty.OverrideDefaultValue<NativeAdMedia>(120);
    }
}
public sealed class NativeAdBody : NativeAdAsset;
public sealed class NativeAdCallToAction : NativeAdAsset;
public sealed class NativeAdAdvertiser : NativeAdAsset;
