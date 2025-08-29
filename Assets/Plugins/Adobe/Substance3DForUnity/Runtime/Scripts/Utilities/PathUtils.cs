using System.IO;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Adobe.Substance
{
    public static class PathUtils
    {
        public const string SubstanceLoaderScriptLocalPath = "\\Runtime\\Scripts\\Utilities";
        public const bool UsingPackageManager = true;

        private static string cachedPluginDirectory;

        public static string GetPluginRoot()
        {
            if (string.IsNullOrEmpty(cachedPluginDirectory))
                cachedPluginDirectory = BuildPluginDirectory();
            return cachedPluginDirectory;
        }

        public static string GetSettingsFilePath()
        {
            return $"{GetPluginRoot()}/Editor/Settings/SubstanceEditorSettings.asset";
        }

        private static string BuildPluginDirectory([CallerFilePath] string callerFilePath = "")
        {
            string localPathToFile = Path.GetDirectoryName(callerFilePath).Remove(0, Application.dataPath.Length - "Assets".Length);
            localPathToFile = localPathToFile.Remove(localPathToFile.Length - PathUtils.SubstanceLoaderScriptLocalPath.Length);
            localPathToFile = localPathToFile.Replace('\\', '/');
            return localPathToFile;
        }
    }
}