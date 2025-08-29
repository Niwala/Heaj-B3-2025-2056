using UnityEngine;
using UnityEditor.AssetImporters;
using Adobe.Substance;
using Adobe.Substance.Input;
using Adobe.SubstanceEditor;
using System.IO;
using System.Collections.Generic;

namespace SamsBackpack.SubstanceReimporter
{
    [ScriptedImporter(1, new string[] { "sbsar" }, new string[] { "sbsar" })]
    public class SubstanceReimproter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            //Read file
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            byte[] rawData = File.ReadAllBytes(ctx.assetPath);

            //Create package object
            SubstancePackage package = ScriptableObject.CreateInstance<SubstancePackage>();
            package.name = fileName;
            package.rawData = rawData;
            ctx.AddObjectToAsset("Main", package);
            ctx.SetMainObject(package);

            //Create graph objects
            int graphCount = Engine.GetFileGraphCount(rawData);
            package.graphs = new SubstanceGraph[graphCount];
            for (int i = 0; i < graphCount; i++)
            {
                SubstanceGraph graph = ScriptableObject.CreateInstance<SubstanceGraph>();
                graph.name = fileName + " : Graph " + i;
                ctx.AddObjectToAsset("Graph_" + i, graph);
                package.graphs[i] = graph;

                SubstanceNativeGraph nativeGraph = Engine.OpenFile(rawData, 0);
                graph.package = package;
                graph.graphID = i;

                List<SubstancePresetInfo> presetList = nativeGraph.GetPresetsList();
                for (int j = 0; j < presetList.Count; j++)
                {
                    Debug.Log(presetList[j].Label + "  " + presetList[j].Description);
                }

                //Inputs
                int inputCount = nativeGraph.GetInputCount();
                graph.inputs = new List<ISubstanceInput>();
                for (int j = 0; j < inputCount; j++)
                {
                    SubstanceInputBase input = nativeGraph.GetInputObject(j);
                    graph.inputs.Add(input);
                }

                //Outputs
                graph.outputs = new List<SubstanceOutputTexture>();
                int outputCount = nativeGraph.GetOutputCount();
                for (int j = 0; j < outputCount; j++)
                {
                    SubstanceOutputDescription desc = nativeGraph.GetOutputDescription(j);
                    graph.outputs.Add(new SubstanceOutputTexture(desc, desc.Identifier));
                }
            }
        }
    }
}