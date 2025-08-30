using System.Diagnostics;

namespace Sych.ShareAssets.Editor.Tools
{
    public static class ScriptPathProvider
    {
        public static string GetScriptPath(string scriptFileName)
        {
            var stackTrace = new StackTrace(true);
            foreach (var frame in stackTrace.GetFrames()!)
            {
                var path = frame.GetFileName();
                if (!string.IsNullOrEmpty(path) && path.EndsWith($"{scriptFileName}.cs"))
                    return path;
            }
            return null;
        }
    }
}