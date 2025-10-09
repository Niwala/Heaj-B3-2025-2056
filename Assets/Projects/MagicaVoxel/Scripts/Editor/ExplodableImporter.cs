using System.Collections.Generic;
using System.IO;

using UnityEditor;

using UnityEngine;
using UnityEngine.VFX;

public class ExplodableImporter : AssetPostprocessor
{
    public void OnPostprocessModel(GameObject gameObject)
    {
        //Check if ply file exist
        string plyFilepath = Path.ChangeExtension(assetPath, "ply");
        if (!File.Exists(plyFilepath))
            return;


        //Check if ply file is loaded
        PlyObject plyObject = AssetDatabase.LoadAssetAtPath<PlyObject>(plyFilepath);
        if (plyObject == null) 
            return;


        //Get resources
        MagicaExplosionResources resources = MagicaExplosionUtility.Resources;

        //Model setup
        MeshFilter filter = gameObject.GetComponentInChildren<MeshFilter>();
        MeshRenderer renderer = gameObject.GetComponentInChildren<MeshRenderer>();
        renderer.sharedMaterial = resources.modelMaterial;
        filter.gameObject.name = "Model";

        //Explosion setup
        GameObject explosionGo = new GameObject("Explosion");
        explosionGo.SetActive(false);
        explosionGo.transform.SetParent(gameObject.transform);

        VoxelExplosion explosion = explosionGo.AddComponent<VoxelExplosion>();
        explosion.plyObject = plyObject;

        VisualEffect effect = explosionGo.AddComponent<VisualEffect>();
        effect.visualEffectAsset = resources.explosionEffect;
        explosion.visualEffect = effect;

        //Explodable
        Explodable explodable = gameObject.AddComponent<Explodable>();
        explodable.model = filter.gameObject;
        explodable.explosion = explosionGo;
    }
}
