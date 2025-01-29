#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

//对于aff读取的支持
namespace PDRO.Utils.Tools
{
    [ScriptedImporter(1, ".aff")]
    public class AffImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var txt = File.ReadAllText(ctx.assetPath);
            var assetsText = new TextAsset(txt);
            ctx.AddObjectToAsset("main obj", assetsText);
            ctx.SetMainObject(assetsText);
        }
    }

    public class AffPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                if (str.EndsWith(".aff"))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(str);
                    AssetDatabase.SetLabels(obj, new string[] { "Aff Chart" });
                }
            }
        }
    }
}

#endif