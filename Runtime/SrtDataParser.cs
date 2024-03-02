using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CaptionsFileParser
{
    public class SrtDataParser : ICaptionDataParser
    {
        const int TIME_FORMAT_LENGTH = 12;

        enum SrtFilePointer
        {
            Index,
            Time,
            RowOneText,
            RowTwoText,
            CaptionEndSpace
        }

        public Queue<Caption> Parse(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError($"Captions parse aborted for <b>{Path.GetFileName(filePath)}</b>");
                return null;
            }

            Queue<Caption> captionsQueue = new Queue<Caption>();

            /* Parse the .srt to a Queue<Caption> */
            try
            {
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    SrtFilePointer filePointer = SrtFilePointer.Index;
                    string readedLine;
                    int fileLineIndex = 0;
                    int tmpIndex;
                    int srtIndex = 1; // .srt starts with index 1
                    TimeSpan entryTime = new TimeSpan();
                    TimeSpan exitTime = new TimeSpan();
                    string subtitleOne = "";
                    string subtitleTwo = "";

                    while ((readedLine = reader.ReadLine()) != null)
                    {
                        fileLineIndex++;

                        switch (filePointer)
                        {
                            case SrtFilePointer.Index:

                                if (string.IsNullOrWhiteSpace(readedLine))
                                {
                                    Debug.LogWarning($"Extra line break detected on line <b>{fileLineIndex}</b>");
                                    continue;
                                }


                                if (int.TryParse(readedLine, out tmpIndex))
                                {
                                    if (srtIndex == tmpIndex)
                                    {
                                        subtitleOne = "";
                                        subtitleTwo = "";
                                        srtIndex++;
                                        filePointer = SrtFilePointer.Time;
                                    }
                                    else
                                    {
                                        Debug.LogError($"Line {fileLineIndex} has an incorrect <b>srt index</b>: It's {readedLine} and it should be {srtIndex}");
                                        return null;
                                    }
                                }
                                else
                                {
                                    Debug.LogError($"Line {fileLineIndex} it should be a <b>index</b>: {readedLine}\nIf the previous line is a line break, please, delete it");
                                    return null;
                                }

                                break;

                            case SrtFilePointer.Time:

                                if (string.IsNullOrWhiteSpace(readedLine))
                                {
                                    Debug.LogWarning($"Extra line break detected on line <b>{fileLineIndex}</b>");
                                    continue;
                                }

                                readedLine = readedLine.Replace(',', '.'); // To TimeSpan format

                                string[] intervalKeyframes = readedLine.Split(new[] { "-->" }, StringSplitOptions.RemoveEmptyEntries);

                                if (intervalKeyframes.Length != 2)
                                {
                                    Debug.LogError($"Line {fileLineIndex} it should be a <b>time interval</b>: {readedLine}\nIf it's a time interval, make sure that has the correct format.\nExample: 00:01:46,860 --> 00:01:49,970");
                                    return null;
                                }

                                // Delete the white spaces
                                intervalKeyframes[0] = intervalKeyframes[0].Trim();
                                intervalKeyframes[1] = intervalKeyframes[1].Trim();

                                if (TIME_FORMAT_LENGTH < intervalKeyframes[0].Length)
                                {
                                    Debug.LogWarning($"Line {fileLineIndex} in entry time has a too large milliseconds number or extra parameters. If it's a large milliseconds number, we gonna get only the three first digits. If it's because a extra parameters, we can't recognize them.");
                                    intervalKeyframes[0] = intervalKeyframes[0].Substring(0, 12);
                                }

                                if (TIME_FORMAT_LENGTH < intervalKeyframes[1].Length)
                                {
                                    Debug.LogWarning($"Line {fileLineIndex} in exit time has a too large milliseconds number or extra parameters. If it's a large milliseconds number, we gonna get only the <b>three first digits</b>. If it's because a extra parameters, we can't recognize them.");
                                    intervalKeyframes[1] = intervalKeyframes[1].Substring(0, 12);
                                }

                                if (!TimeSpan.TryParse(intervalKeyframes[0], out entryTime))
                                {
                                    Debug.LogError($"Line {fileLineIndex}: incorrect <b>entry time format</b>: {readedLine}");
                                    return null;
                                }

                                if (!TimeSpan.TryParse(intervalKeyframes[1], out exitTime))
                                {
                                    Debug.LogError($"Line {fileLineIndex}: incorrect <b>exit time format</b>: {readedLine}");
                                    return null;
                                }

                                filePointer = SrtFilePointer.RowOneText;
                                break;

                            case SrtFilePointer.RowOneText:

                                if (string.IsNullOrWhiteSpace(readedLine))
                                {
                                    Debug.LogError($"Subtitle row one in line {fileLineIndex} can't be emptyLine. If it's a line break, delete it please.");
                                    return null;
                                }

                                subtitleOne = readedLine;
                                filePointer = SrtFilePointer.RowTwoText;
                                break;

                            case SrtFilePointer.RowTwoText:
                                subtitleTwo = readedLine;

                                filePointer = string.IsNullOrWhiteSpace(readedLine) ? SrtFilePointer.Index : SrtFilePointer.CaptionEndSpace; // Jump directly to Index if the second row is white space

                                if (string.IsNullOrWhiteSpace($"{subtitleOne}{subtitleTwo}"))
                                {
                                    Debug.LogError("The caption text is empty. It's useless add this subtitle");
                                    break;
                                }

                                if (exitTime < entryTime)
                                {
                                    Debug.LogError("Interval inconsistency. The parameter <b>exitTime</b> must be greater or equal than <b>entryTime</b>. Subtitle not added.");
                                    break;
                                }

                                captionsQueue.Enqueue(new Caption(subtitleOne, subtitleTwo, entryTime, exitTime));


                                //Debug.Log($"Adding:\n- Subtitle one: <b>{subtitleOne}</b>\n- Subtitle two: <b>{subtitleTwo}</b>\n- Entry time: <b>{entryTime}</b>\n- Exit time: <b>{exitTime}</b>");

                                break;

                            case SrtFilePointer.CaptionEndSpace:

                                if (!string.IsNullOrWhiteSpace(readedLine))
                                {
                                    Debug.LogError($"Invalid srt format. Line <b>{fileLineIndex}</b> should be a white space.\nCancelling the parse.");
                                    return null;
                                }

                                filePointer = SrtFilePointer.Index;
                                break;

                            default:
                                Debug.LogError($"Pointer type not implemented: <b>{filePointer}</b>");
                                return null;
                        }
                    }

                    if (filePointer == SrtFilePointer.RowTwoText) // add if the last caption if just have one row
                    {
                        captionsQueue.Enqueue(new Caption(subtitleOne, subtitleTwo, entryTime, exitTime));
                        Debug.Log($"Adding LAST:\n- Subtitle one: <b>{subtitleOne}</b>\n- Subtitle two: <b>{subtitleTwo}</b>\n- Entry time: <b>{entryTime}</b>\n- Exit time: <b>{exitTime}</b>");
                    }
                    else if (filePointer != SrtFilePointer.Index && filePointer != SrtFilePointer.CaptionEndSpace)
                    {
                        Debug.LogWarning($"Can't add the last caption with the srt index: <b>{srtIndex - 1}</b>");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Debug.LogError($"You don't have access to this file. Try to enable via file properties or your user permissions. Path: <b>{filePath}</b>");
                return null;
            }

            // Debug.Log($"<b>{Path.GetFileName(filePath)}</b> was parsed to <b>{captionsQueue.ToArray()}</b>.");
            return captionsQueue;
        }
    }
}
