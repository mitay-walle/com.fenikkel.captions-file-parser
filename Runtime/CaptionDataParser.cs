using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CaptionsFileParser
{
	public class CaptionDataParser : ICaptionDataParser
	{
		public enum CaptionFileType
		{
			None,
			SRT,
			TXT
		}

		public Queue<Caption> Parse(TextAsset textAsset)
		{
			return Parse(Application.dataPath + AssetDatabase.GetAssetPath(textAsset));
		}

		public Queue<Caption> Parse(string filePath)
		{
			// Check file type
			CaptionFileType fileExtension = GetCaptionsFileType(filePath);

			if (fileExtension.Equals(CaptionFileType.None))
			{
				return null;
			}

			// Check character codification
			if (!IsUtf8(filePath))
			{
				Debug.LogWarning($"Only UTF-8 encoding allowed. Please save the file with UTF-8 encoding: {filePath}");
				return null;
			}

			// Call the correct captions parser
			ICaptionDataParser parser = null;

			switch (fileExtension)
			{
				case CaptionFileType.TXT:
					goto case CaptionFileType.SRT;

				case CaptionFileType.SRT:
					parser = new SrtDataParser();
					break;

				default:
					Debug.LogError($"<b>{fileExtension}</b> parser still not implemented.");
					return null;
			}

			return parser.Parse(filePath);
		}

		private CaptionFileType GetCaptionsFileType(string filePath)
		{
			if (Directory.Exists(filePath))
			{
				Debug.LogWarning($"Folder path not allowed: {filePath}");
				return CaptionFileType.None;
			}

			if (!File.Exists(filePath))
			{
				Debug.LogWarning($"Incorrect file path: {filePath}");
				return CaptionFileType.None;
			}

			string fileExtension = Path.GetExtension(filePath);

			if (string.IsNullOrEmpty(fileExtension))
			{
				Debug.LogWarning($"Error during the file extension extraction: {filePath}");
				return CaptionFileType.None;
			}

			fileExtension = fileExtension.Substring(1); // takes out the dot "."
			fileExtension = fileExtension.ToUpper();

			CaptionFileType captionType;

			if (Enum.TryParse(fileExtension, out captionType))
			{
				switch (captionType)
				{
					case CaptionFileType.TXT:
						Debug.LogWarning("Extension is \".txt\". It is going to be treated like a \".srt\"");
						goto case CaptionFileType.SRT;

					case CaptionFileType.SRT:
						return CaptionFileType.SRT;

					default:
						Debug.LogError($"CaptionType not implemented: <b>{fileExtension}</b>");
						return CaptionFileType.None;
				}
			}
			else
			{
				Debug.LogError($"File extension not permitted: <b>{fileExtension}</b>");
				return CaptionFileType.None;
			}
		}

		private bool IsUtf8(string filePath)
		{
			/*
			    - UTF-8 has ASCII characters (1 byte characters) and also has special characters that have 2, 3 or 4 bytes.
			    - This method checks if the file has 2 or more bytes.
			    - It can give false positive if a file with a diferent codification meets these conditions by chance (for example a direct access). Or, the file doesn't have any special characters(��^)
			    - Works even if the file has BOM (ByteOrderMarks)
			*/

			if (Directory.Exists(filePath))
			{
				Debug.LogWarning("Can't check the encoding of a <b>folder</b>");
				return false;
			}

			try
			{
				byte[] bytes = File.ReadAllBytes(filePath);

				for (int i = 0; i < bytes.Length; i++)
				{
					byte b = bytes[i];
					if (b <= 0x7F) // ASCII
					{
						continue;
					}

					if (b >= 0xC2 && b <= 0xDF) // 2-byte sequence
					{
						if (i + 1 >= bytes.Length || bytes[i + 1] < 0x80 || bytes[i + 1] > 0xBF)
						{
							return false;
						}

						i++;
					}
					else if (b >= 0xE0 && b <= 0xEF) // 3-byte sequence
					{
						if (i + 2 >= bytes.Length || bytes[i + 1] < 0x80 || bytes[i + 1] > 0xBF || bytes[i + 2] < 0x80 || bytes[i + 2] > 0xBF)
						{
							return false;
						}

						i += 2;
					}
					else if (b >= 0xF0 && b <= 0xF4) // 4-byte sequence
					{
						if (i + 3 >= bytes.Length || bytes[i + 1] < 0x80 || bytes[i + 1] > 0xBF || bytes[i + 2] < 0x80 || bytes[i + 2] > 0xBF || bytes[i + 3] < 0x80 || bytes[i + 3] > 0xBF)
						{
							return false;
						}

						i += 3;
					}
					else
					{
						return false;
					}
				}
			}
			catch (UnauthorizedAccessException)
			{
				Debug.LogError($"You don't have access to this file. Try to enable via file properties or your user permissions. Path: <b>{filePath}</b>");
				return false;
			}

			return true;
		}
	}
}