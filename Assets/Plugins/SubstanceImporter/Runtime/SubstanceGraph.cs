using Adobe.Substance;
using Adobe.Substance.Input;

using System.Collections.Generic;

using UnityEngine;

namespace SamsBackpack.SubstanceReimporter
{
    public class SubstanceGraph : ScriptableObject
    {
        public SubstancePackage package;

        public int graphID;

        [SerializeReference]
        public List<ISubstanceInput> inputs;

        [SerializeField]
        public List<SubstanceOutputTexture> outputs;

        private SubstanceNativeGraph Graph
        {
            get
            {
                if (graph == null)
                    graph = package.LoadGraph(graphID);
                return graph;
            }
        }
        private SubstanceNativeGraph graph;

        public void SetSeed(int seed)
        {
            Graph.SetInputInt(1, seed);
        }

        public void SetOutputSize(int width, int height)
        {
            Graph.SetInputInt2(0, new Vector2Int(width, height));
        }

        public void SetTexture(string name, Texture2D texture)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].Description.Identifier == name)
                {
                    if (texture == null)
                        Graph.SetInputTexture2DNull(inputs[i].Index);
                    else
                        Graph.SetInputTexture2D(inputs[i].Index, texture);
                    return;
                }
            }
            throw new System.Exception($"Input name {name} (Texture2D) not found on {name}.");
        }

        public void Render(SubstancePropertyBlock substance)
        {
            Material outputMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

            //Reset outputs
            int outputCount = Graph.GetOutputCount();
            outputs = new List<SubstanceOutputTexture>();
            for (int i = 0; i < outputCount; i++)
            {
                SubstanceOutputDescription desc = Graph.GetOutputDescription(i);
                desc.Channel = "mask";
                outputs.Add(new SubstanceOutputTexture(desc, "Tex" + i));
            }

            SubstanceGraphSO tempSO = ScriptableObject.CreateInstance<SubstanceGraphSO>();
            tempSO.Input = inputs;
            tempSO.Output = outputs;
            tempSO.OutputMaterial = outputMaterial;
            tempSO.GenerateAllOutputs = true;
            tempSO.RuntimeInitialize(Graph, true);
            outputs = tempSO.Output;
            DestroyImmediate(tempSO);
        }

        public Texture2D GetOutput(int id)
        {
            return outputs[id].OutputTexture;
        }
    }
}