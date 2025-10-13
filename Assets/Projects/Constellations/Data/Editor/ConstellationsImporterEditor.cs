using UnityEditor;
using UnityEditor.AssetImporters;

using UnityEngine;

[CustomEditor(typeof(ConstellationsImporter))]
public class ConstellationsImporterEditor : ScriptedImporterEditor
{
    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void DrawPreview(Rect previewArea)
    {
        ConstellationsImporter importer = (ConstellationsImporter)target;
        Constellations constellations = importer.constellations;
        if (constellations == null)
            return;

        float widthRemap = 1.0f / constellations.width * 60;
        float heightRemap = 1.0f / constellations.height * 60;
        float xOffset = 0;
        float yOffset = 20f;

        Handles.BeginGUI();
        GUILayout.BeginArea(previewArea);

        foreach (var path in constellations.paths)
        {
            Vector3[] polyLine = new Vector3[path.positions.Length];
            for (int i = 0; i < path.positions.Length; i++)
            {
                Vector2 p = path.positions[i];

                polyLine[i] = new Vector3(p.x * widthRemap, previewArea.height - p.y * heightRemap + yOffset, 0);
            }
            Handles.DrawPolyLine(polyLine);
        }

        GUILayout.EndArea();
        Handles.EndGUI();
    }
}
