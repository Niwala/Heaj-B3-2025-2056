using System.Collections.Generic;
using System.IO;

using UnityEditor.AssetImporters;

using UnityEngine;

[ScriptedImporter(0, ext)]
public class PlyImporter : ScriptedImporter
{
    public const string ext = "ply";

    private const string elementCount = "element vertex ";
    private const string endHeader = "end_header";
    private const float gammaCorrection = 2.2f;

    public float scaling = 0.1f;
    public bool useGammaCorrection = true;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        PlyObject main = ScriptableObject.CreateInstance<PlyObject>();
        main.name = Path.GetFileNameWithoutExtension(assetPath);

        //Read file
        string plyFile = File.ReadAllText(assetPath);

        using (StringReader reader = new StringReader(plyFile))
        {
            int voxelID = 0;
            bool inHeader = true;

            while (reader.Peek() > 0)
            {
                string line = reader.ReadLine();

                //Read metadata
                if (inHeader)
                {
                    //Element count
                    if (line.StartsWith(elementCount))
                    {
                        int voxelCount = int.Parse(line.Remove(0, elementCount.Length));
                        main.count = voxelCount;
                        main.colors = new Vector3[voxelCount];
                        main.positions = new Vector3[voxelCount];
                    }

                    //Header end
                    else if (line == endHeader)
                    {
                        inHeader = false;
                    }
                }

                //Read voxel
                else
                {
                    (Vector3 position, Vector3 color) = ReadVoxelLine(line);
                    main.positions[voxelID] = position * 0.1f;
                    main.colors[voxelID] = color;
                    voxelID++;
                }
            }
        }

        //Add main object
        ctx.AddObjectToAsset("main", main);
        ctx.SetMainObject(main);

    }

    private static (Vector3 position, Vector3 color) ReadVoxelLine(string line)
    {
        string[] values = line.Split(' ');

        Vector3 position = new Vector3(
            -int.Parse(values[0]),
            -int.Parse(values[2]),
            int.Parse(values[1]));

        Vector3 color = new Vector3(
            Mathf.Pow(int.Parse(values[3]) / 255.0f, gammaCorrection),
            Mathf.Pow(int.Parse(values[4]) / 255.0f, gammaCorrection),
            Mathf.Pow(int.Parse(values[5]) / 255.0f, gammaCorrection));

        return (position, color);
    }
}
