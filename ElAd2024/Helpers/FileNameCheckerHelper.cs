namespace ElAd2024.Helpers;
public class FileNameCheckerHelper
{
    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
    private static readonly string[] ReservedNames =
    [
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    ];

    public static bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        if (fileName.IndexOfAny(InvalidFileNameChars) >= 0)
        {
            return false;
        }

        if (ReservedNames.Contains(fileName.ToUpperInvariant()))
        {
            return false;
        }

        if (fileName.EndsWith(" ") || fileName.EndsWith("."))
        {
            return false;
        }

        // Optional: Add additional checks such as length restrictions here

        return true;
    }
}