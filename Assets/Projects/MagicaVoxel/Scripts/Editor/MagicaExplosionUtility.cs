using System.IO;
using System.Runtime.CompilerServices;

using UnityEditor;

using UnityEngine;

public static class MagicaExplosionUtility
{
    //Properties
    public static MagicaExplosionResources Resources
    {
        get
        {
            if (resources == null)
                resources = LoadResources();
            return resources;
        }
    }

    //Fields
    private static MagicaExplosionResources resources;
    private const string resourcesPath = "\\Data\\Magica Explosion Resources.asset";


    private static MagicaExplosionResources LoadResources()
    {
        string filepath = GetPluginPath() + resourcesPath;
        return AssetDatabase.LoadAssetAtPath<MagicaExplosionResources>(filepath);
    }

    private static string GetPluginPath([CallerFilePath] string filepath = "")
    {
        string directory = Directory.GetParent(filepath).Parent.Parent.FullName;
        string localPath = directory.Remove(0, Application.dataPath.Length - "Assets".Length);
        return localPath;
    }
}
