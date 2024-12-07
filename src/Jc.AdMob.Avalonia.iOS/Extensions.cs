using System;
using System.Runtime.InteropServices;

using ObjCRuntime;
using Foundation;
using CoreGraphics;
using UIKit;

namespace Google.MobileAds {
	[Preserve (AllMembers = true)]
	public partial class AdSizeCons {
		//GAD_EXTERN GADAdSize GADCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth (CGFloat width);
		[DllImport ("__Internal", EntryPoint = "GADCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth")]
		public static extern AdSize GetCurrentOrientationAnchoredAdaptiveBannerAdSize (nfloat width);
	}
}
