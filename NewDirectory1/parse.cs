namespace ModAge;

using System;
using System.Collections.Generic;

public class ParsedLog
{
    public Dictionary<string, Mod> Mods { get; set; } = new Dictionary<string, Mod>();
    public Version ValheimVersion { get; set; }
    public Version BepinexVersion { get; set; }
    public Version BepinexThunderstoreVersion { get; set; }
    
    private static Version ParseVersion(string prefix, bool anywhere, string line)
    {
        if (anywhere && line.Contains(prefix))
        {
            string versionString = line.Split(new[] { prefix }, StringSplitOptions.None)[1].Trim().Split(' ')[0];
            return new Version(versionString);
        }
        else if (line.StartsWith(prefix))
        {
            string versionString = line.Split(new[] { prefix }, StringSplitOptions.None)[1].Trim().Split(' ')[0];
            return new Version(versionString);
        }

        return null; // equivalent of Python's None
    }

    
    public static ParsedLog ParseLocal(string localText, bool isLogFile)
    {
        ParsedLog parsedLog = new ParsedLog();
        string[] lines = localText.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            // Remaining logic goes here...

            // Skipping the mod parsing for now, as I'm assuming "Mod" and "clean_name" need their own definitions
        }

        return parsedLog;
    }

    
    
}
