using UnityEditor.AssetImporters;

using UnityEngine;
using UnityEditor;

namespace SamsBackpack.SubstanceReimporter
{
    [CustomEditor(typeof(SubstanceReimproter))]
    public class SubstanceReimporterEditor : ScriptedImporterEditor
    {
        public override bool showImportedObject => false;
        protected override bool ShouldHideOpenButton()
        {
            return true;
        }



    }
}
