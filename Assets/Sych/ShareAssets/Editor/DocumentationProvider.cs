using System.IO;
using Sych.ShareAssets.Editor.Tools;

namespace Sych.ShareAssets.Editor
{
    public static class DocumentationProvider
    {
        public static string GetFilePath()
        {
            try
            {
                var scriptPath = ScriptPathProvider.GetScriptPath(nameof(DocumentationProvider));
                if (string.IsNullOrEmpty(scriptPath))
                    return null;

                var scriptDirectoryPath = Path.GetDirectoryName(scriptPath);
                if (scriptDirectoryPath == null)
                    return null;

                var assetsDirectoryPath = Path.GetDirectoryName(scriptDirectoryPath);
                if (assetsDirectoryPath == null)
                    return null;

                var filePath = Path.Combine(assetsDirectoryPath, "Readme.pdf");
                return File.Exists(filePath) ? filePath : null;
            }
            catch
            {
                return null;
            }
        }
    }
}