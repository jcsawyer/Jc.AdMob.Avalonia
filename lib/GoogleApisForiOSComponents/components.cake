// Firebase artifacts available to be built. These artifacts generate NuGets.
Artifact GOOGLE_MOBILE_ADS_ARTIFACT   = new Artifact ("Google.MobileAds",             "10.0.0-preview200", "11.0", ComponentGroup.Google, csprojName: "MobileAds");
Artifact GOOGLE_UMP_ARTIFACT		  = new Artifact ("Google.UserMessagingPlatform", "1.1.0.1", "11.0", ComponentGroup.Google, csprojName: "UserMessagingPlatform");

var ARTIFACTS = new Dictionary<string, Artifact> {
	{ "Google.MobileAds",             GOOGLE_MOBILE_ADS_ARTIFACT },
	{ "Google.UserMessagingPlatform", GOOGLE_UMP_ARTIFACT },
};

void SetArtifactsDependencies ()
{
	GOOGLE_MOBILE_ADS_ARTIFACT.Dependencies   = null;
	GOOGLE_UMP_ARTIFACT.Dependencies		  = null;
}

void SetArtifactsPodSpecs ()
{
	GOOGLE_MOBILE_ADS_ARTIFACT.PodSpecs = new [] {
		PodSpec.Create ("Google-Mobile-Ads-SDK", "23.4.0")
	};
	
	GOOGLE_UMP_ARTIFACT.PodSpecs = new [] {
		PodSpec.Create ("GoogleUserMessagingPlatform", "2.7.0")
	};
}

void SetArtifactsExtraPodfileLines ()
{
	var dynamicFrameworkLines = new [] {	
		"=begin",
		"This override the static_framework flag to false,",
		"in order to be able to build dynamic frameworks.",
		"=end",
		"pre_install do |installer|",
		"\tinstaller.pod_targets.each do |pod|",
		"\tputs \"Forcing a static_framework to false for #{pod.name}\"",
		"\t\tif Pod::VERSION >= \"1.7.0\"",
		"\t\t\tif pod.build_as_static?",
		"\t\t\t\tdef pod.build_as_static?; false end",
		"\t\t\t\tdef pod.build_as_static_framework?; false end",
		"\t\t\t\tdef pod.build_as_dynamic?; true end",
		"\t\t\t\tdef pod.build_as_dynamic_framework?; true end",
		"\t\t\tend",
		"\t\telse",
		"\t\t\tdef pod.static_framework?; false end",
		"\t\tend",
		"\tend",
		"end",
	};

	var avoidBundleSigning = new [] {
		"=begin",
		"It seems that there is an issue with bundles and Xcode 14, it asks for your Team ID to sign them when building.",
		"Here's a workaround for this: https://github.com/CocoaPods/CocoaPods/issues/8891#issuecomment-1201465446",
		"=end",
		"post_install do |installer|",
		"\tinstaller.pods_project.targets.each do |target|",
		"\t\tif target.respond_to?(:product_type) and target.product_type == \"com.apple.product-type.bundle\"",
      	"\t\t\ttarget.build_configurations.each do |config|",
		"\t\t\t\tputs \"Setting 'CODE_SIGNING_ALLOWED' to 'NO' for #{target.name}\"",
        "\t\t\t\tconfig.build_settings['CODE_SIGNING_ALLOWED'] = 'NO'",
      	"\t\t\tend",
    	"\t\tend",
		"\tend",
		"end",
	};

	var inAppMessagingWorkaround = new [] {
		"post_install do |installer|",
		"\tinstaller.pods_project.targets.each do |target|",
		"\t\tif target.name == \"FirebaseInAppMessaging\"",
		"\t\t\ttarget.build_configurations.each do |config|",
		"\t\t\t\tif config.name == 'Release'",
		"\t\t\t\t\tputs \"Linking missing 'GoogleUtilities' framework to #{target.name}\"",
		"\t\t\t\t\tconfig.build_settings['OTHER_LDFLAGS'] ||= ['$(inherited)','-framework \"GoogleUtilities\"']",
		"\t\t\t\tend",
		"\t\t\tend",
		"\t\tend",
		"\t\tif target.respond_to?(:product_type) and target.product_type == \"com.apple.product-type.bundle\"",
      	"\t\t\ttarget.build_configurations.each do |config|",
		"\t\t\t\tputs \"Setting 'CODE_SIGNING_ALLOWED' to 'NO' for #{target.name}\"",
        "\t\t\t\tconfig.build_settings['CODE_SIGNING_ALLOWED'] = 'NO'",
      	"\t\t\tend",
    	"\t\tend",
		"\tend",
		"end",
	};
}

void SetArtifactsSamples ()
{
	GOOGLE_MOBILE_ADS_ARTIFACT.Samples                = new [] { "MobileAdsExample" };
}
