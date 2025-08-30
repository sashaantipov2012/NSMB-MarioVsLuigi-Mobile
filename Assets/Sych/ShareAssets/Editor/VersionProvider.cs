using System.IO;
using Sych.ShareAssets.Editor.Tools;

namespace Sych.ShareAssets.Editor
{
    public static class VersionProvider
    {
        public static string GetVersion()
        {
            try
            {
                var versionFilePath = GetVersionFilePath();
                if (string.IsNullOrEmpty(versionFilePath) || !File.Exists(versionFilePath))
                    return "Unknown";

                return File.ReadAllText(versionFilePath).Trim();
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string GetVersionFilePath()
        {
            var scriptPath = ScriptPathProvider.GetScriptPath(nameof(VersionProvider));
            if (string.IsNullOrEmpty(scriptPath))
                return null;

            var scriptDirectoryPath = Path.GetDirectoryName(scriptPath);
            if(scriptDirectoryPath == null)
                return null;
            
            var assetsDirectoryPath = Path.GetDirectoryName(scriptDirectoryPath);
            if(assetsDirectoryPath == null)
                return null;
            
            var versionFilePath = Path.Combine(assetsDirectoryPath, "Version.txt");
            return File.Exists(versionFilePath) ? versionFilePath : null;
        }
    }
}