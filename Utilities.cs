using System;

namespace ModAge;

public class Utilities
{
    internal static System.Version ParseVersion(string? input)
    {
        try
        {
            System.Version ver = System.Version.Parse(input);
            return ver;
        }
        catch (ArgumentNullException)
        {
            ModAgePlugin.ModAgeLogger.LogError("Error: String to be parsed is null.");
        }
        catch (ArgumentOutOfRangeException)
        {
            ModAgePlugin.ModAgeLogger.LogError($"Error: Negative value in '{input}'.");
        }
        catch (ArgumentException)
        {
            ModAgePlugin.ModAgeLogger.LogError($"Error: Bad number of components in '{input}'.");
        }
        catch (FormatException)
        {
            ModAgePlugin.ModAgeLogger.LogError($"Error: Non-integer value in '{input}'.");
        }
        catch (OverflowException)
        {
            ModAgePlugin.ModAgeLogger.LogError($"Error: Number out of range in '{input}'.");
        }

        return System.Version.Parse(ModAgePlugin.ModVersion);
    }
}