using System.IO;

using UnityEngine;
using UnityEditor.AssetImporters;

[ScriptedImporter(1, new string[] { }, new string[] { "md" }, -200)]
public class MarkdownImporter : ScriptedImporter
{
    public MarkdownDocument document => _document;

    [SerializeField]
    private MarkdownDocument _document;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        string file = File.ReadAllText(ctx.assetPath);
        _document = ScriptableObject.CreateInstance<MarkdownDocument>();
        _document.file = file;
        ctx.AddObjectToAsset("Document", _document);
        ctx.SetMainObject(_document);
    }
}