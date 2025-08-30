using System;
using System.Xml.Serialization;

namespace Sych.ShareAssets.Editor.Overview
{
    [Serializable]
    [XmlRoot("settings")]
    public sealed class AssetOverviewSettings
    {
        [XmlElement("plugin_name")] public string PluginName { get; set; }
        [XmlElement("plugin_description")] public string PluginDescription { get; set; }
        [XmlElement("support_email")] public string SupportEmail { get; set; }
        [XmlElement("current_asset_store_link")] public string CurrentAssetStoreLink { get; set; }
        [XmlElement("all_asset_store_link")] public string AllAssetStoreLink { get; set; }
        [XmlElement("license")] public string License { get; set; }
        [XmlElement("is_introduction_enabled")] public bool IsIntroductionEnabled { get; set; }
        [XmlElement("android_bundle_main_type")] public string AndroidBundleMainType { get; set; }
        [XmlElement("ios_bundle_main_type")] public string IosBundleMainType { get; set; }
        [XmlElement("android_bundle_main_assembly")] public string AndroidBundleMainAssembly { get; set; }
        [XmlElement("ios_bundle_main_assembly")] public string IosBundleMainAssembly { get; set; }
    }
}