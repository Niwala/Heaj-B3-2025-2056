using Adobe.Substance;

using System.Collections.Generic;

using UnityEngine;

namespace SamsBackpack.SubstanceReimporter
{
    public class SubstancePackage : ScriptableObject
    {
        [SerializeField, HideInInspector]
        public byte[] rawData;

        public SubstanceGraph[] graphs;

        public SubstanceNativeGraph LoadGraph(int id)
        {
            return Engine.OpenFile(rawData, id);
        }
    }
}