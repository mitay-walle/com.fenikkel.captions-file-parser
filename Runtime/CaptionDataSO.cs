using System.Collections.Generic;
using UnityEngine;

namespace CaptionsFileParser
{
	[CreateAssetMenu]
	public class CaptionDataSO : ScriptableObject, ISerializationCallbackReceiver
	{
		public Queue<Caption> Captions = new Queue<Caption>();
		[SerializeField] private List<Caption> _captionsSerialized = new();

		public void OnBeforeSerialize()
		{
			_captionsSerialized.Clear();
			if (Captions == null) Captions = new();
			_captionsSerialized.AddRange(Captions);
		}

		public void OnAfterDeserialize()
		{
			Captions.Clear();
			foreach (Caption caption in _captionsSerialized)
			{
				Captions.Enqueue(caption);
			}
		}
	}
}