using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(MarkdownImporter))]
public class MarkdownDocumentEditor : ScriptedImporterEditor
{
    public override VisualElement CreateInspectorGUI()
    {
        MarkdownImporter importer = target as MarkdownImporter;
        importer.document.binding = new SerializedObject(importer.document);
        VisualElement root = importer.document.GenerateUI();
        root.style.paddingTop = 10;
        return root;
    }

    protected override bool needsApplyRevert => false;
    protected override bool useAssetDrawPreview => false;
    public override bool showImportedObject => false;

    public override bool UseDefaultMargins()
    {
        return true;
    }

    public override bool HasPreviewGUI()
    {
        return false;
    }
}