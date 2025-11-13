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
			CaptionDataSO subAsset = ScriptableObject.CreateInstance<CaptionDataSO>();

			string projectPath = Path.GetDirectoryName(Application.dataPath);
			string path = Path.Combine(projectPath, assetPath);

			subAsset.Captions = new SrtDataParser().Parse(path);
			ctx.AddObjectToAsset("ParsedSO", subAsset);
			ctx.SetMainObject(subAsset);
		}
	}
}