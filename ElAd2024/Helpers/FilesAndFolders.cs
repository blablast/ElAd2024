using System.Diagnostics;
using ElAd2024.Services;
using Windows.System;
namespace ElAd2024.Helpers;
public class FilesAndFolders
{
    public static async Task OpenFolderAsync(string folderPath)
    {
        folderPath = Path.GetDirectoryName(folderPath) ?? string.Empty;
        if (!Directory.Exists(folderPath))
        {
            throw new ArgumentException("Folder path is null or empty", nameof(folderPath));
        }

        // Launch the folder
        var isLaunched = await Launcher.LaunchFolderPathAsync(folderPath);

        if (!isLaunched)
        {
            // Handle the case where the folder couldn't be opened
            Debug.WriteLine($"Failed to open the folder: {folderPath}");
        }
    }

}
