using System;

namespace CaptionsFileParser
{ 

[Serializable]
public struct Caption
{
    public string FirstRow;
    public string SecondRow;
    public TimeSpan EntryTime;
    public TimeSpan ExitTime;

    public Caption(string subtitleOne, string subtitleTwo, TimeSpan entryTime, TimeSpan exitTime)
    {
        FirstRow = subtitleOne;
        SecondRow = subtitleTwo;
        EntryTime = entryTime;
        ExitTime = exitTime;
    }
}
}
