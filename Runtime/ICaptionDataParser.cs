using System.Collections.Generic;

namespace CaptionsFileParser
{
    public interface ICaptionDataParser
    {
        Queue<Caption> Parse(string pathToFile); // cambiar a matriz
    }
}
