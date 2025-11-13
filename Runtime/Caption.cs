using System;

namespace CaptionsFileParser
{
	[Serializable]
	public struct Caption
	{
		public string FirstRow;
		public string SecondRow;
		public float EntryTime;
		public float ExitTime;

		public Caption(string subtitleOne, string subtitleTwo, TimeSpan entryTime, TimeSpan exitTime)
		{
			FirstRow = subtitleOne;
			SecondRow = subtitleTwo;
			EntryTime = (float)entryTime.TotalSeconds;
			ExitTime = (float)exitTime.TotalSeconds;
		}
	}
}