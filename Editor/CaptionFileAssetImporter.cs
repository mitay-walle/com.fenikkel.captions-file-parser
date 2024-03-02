using UnityEngine;
using System.IO;
using UnityEditor.AssetImporters;

/* 
    We import the caption files into Unity as if they were a TextAsset.
 */
namespace CaptionsFileParser
{
    [ScriptedImporter(1, "srt")]
    public class SrtImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            TextAsset subAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("text", subAsset);
            ctx.SetMainObject(subAsset);
        }
    }
}

