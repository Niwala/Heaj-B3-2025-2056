using UnityEditor.AssetImporters;

using UnityEngine;

[ScriptedImporter(0, ext)]
public class ContellationImporter : ScriptedImporter
{
    public const string ext = "svg";

    public override void OnImportAsset(AssetImportContext ctx)
    {
        //Create main object
        Constellation main = ScriptableObject.CreateInstance<Constellation>();
        ctx.AddObjectToAsset("main", main);
        ctx.SetMainObject(main);

        //Read file
        SvgReader.Read(ctx.assetPath);
    }
}
