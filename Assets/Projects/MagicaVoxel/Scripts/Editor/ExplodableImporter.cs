using System.IO;

using UnityEditor;

using UnityEngine;
using UnityEngine.VFX;

public class ExplodableImporter : AssetPostprocessor
{
    public Material material;

    private void OnPostprocessModel(GameObject gameObject)
    {
        //Try to find poly object
        string ext = Path.GetExtension(assetPath);
        string plyFilePath = assetPath.Remove(assetPath.Length - ext.Length) + "." + PlyImporter.ext;
        if (!File.Exists(plyFilePath))
            return;

        PlyObject plyObject = AssetDatabase.LoadAssetAtPath<PlyObject>(plyFilePath);
        if (plyObject == null)
            return;

        
        //Create explosion object
        GameObject explosionGo = new GameObject("Explosion");
        explosionGo.transform.SetParent(gameObject.transform);
        VisualEffect effect = explosionGo.AddComponent<VisualEffect>();


        //Add explosion component
        VoxelExplosion explosion = gameObject.AddComponent<VoxelExplosion>();
        explosion.visualEffect = effect;
        explosion.plyObject = plyObject;

    }
}
