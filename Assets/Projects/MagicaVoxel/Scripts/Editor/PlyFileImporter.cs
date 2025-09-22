using System.IO;

using Unity.Mathematics;

using UnityEditor.AssetImporters;

using UnityEngine;

[ScriptedImporter(0, extension)]
public class PlyFileImporter : ScriptedImporter
{
    public const string extension = "ply";

    private const string elementVertex = "element vertex ";
    private const string headerEnd = "end_header";
    const float scale = 0.1f;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        PlyObject main = ScriptableObject.CreateInstance<PlyObject>();

        bool inHeader = true;
        int voxelID = 0;

        using (StringReader reader = new StringReader(File.ReadAllText(ctx.assetPath)))
        {
            while (reader.Peek() > 0)
            {
                string line = reader.ReadLine();

                //Header
                if (inHeader)
                {
                    if (line.StartsWith(elementVertex))
                    {
                        int voxelCount = int.Parse(line.Remove(0, elementVertex.Length));
                        main.positions = new float3[voxelCount];
                        main.colors = new float3[voxelCount];
                        continue;
                    }

                    if (line == headerEnd)
                    {
                        inHeader = false;
                    }

                }

                //Voxels
                else
                {
                    (main.positions[voxelID], main.colors[voxelID]) = FromString(line);
                    voxelID++;
                }
            }
        }

        ctx.AddObjectToAsset("main", main);
        ctx.SetMainObject(main);
    }

    public static (float3 position, float3 color) FromString(string value)
    {
        string[] values = value.Split(' ');
        return (new float3(int.Parse(values[0]), int.Parse(values[2]), int.Parse(values[1])) * scale,
            new float3(int.Parse(values[3]) / 255.0f, int.Parse(values[4]) / 255.0f, int.Parse(values[5]) / 255.0f));
    }
}