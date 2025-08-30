using System.IO;
using System.Xml.Serialization;
using Sych.ShareAssets.Editor.Tools;

namespace Sych.ShareAssets.Editor.Overview
{
    internal static class AssetOverviewSettingsProvider
    {
        public static AssetOverviewSettings GetSettings()
        {
            try
            {
                var settingsPath = GetFilePath();
                if (settingsPath == null)
                    return new AssetOverviewSettings();

                var serializer = new XmlSerializer(typeof(AssetOverviewSettings));
                using var stream = new FileStream(settingsPath, FileMode.Open);
                return (AssetOverviewSettings)serializer.Deserialize(stream);
            }
            catch
            {
                return new AssetOverviewSettings();
            }
        }
        
        public static void SatSettings(AssetOverviewSettings settings)
        {
            try
            {
                var settingsPath = GetFilePath();
                if (settingsPath == null)
                    return;

                var serializer = new XmlSerializer(typeof(AssetOverviewSettings));
                using var stream = new FileStream(settingsPath, FileMode.Create);
               serializer.Serialize(stream, settings);
            }
            catch
            {
                // ignored
            }
        }

        private static string GetFilePath()
        {
            var scriptPath = ScriptPathProvider.GetScriptPath(nameof(AssetOverviewSettingsProvider));
            if (string.IsNullOrEmpty(scriptPath))
                return null;

            var scriptDirectoryPath = Path.GetDirectoryName(scriptPath);
            if (scriptDirectoryPath == null)
                return null;

            var filePath = Path.Combine(scriptDirectoryPath, "settings.xml");
            return File.Exists(filePath) ? filePath : null;
        }
    }
}