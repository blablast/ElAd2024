using System.IO.Compression;
using Windows.Storage;

namespace ElAd2024.Helpers;

public class ZipFilesHelper
{
    public static async Task CreateZipFileAsync(List<string> filePaths, string destinationZipFile)
    {
        // Create a zip file at the specified destination path
        var documentsFolder = KnownFolders.DocumentsLibrary;
        var storageFile = await documentsFolder.CreateFileAsync(destinationZipFile, CreationCollisionOption.ReplaceExisting);

        // Open the storage file as a stream
        using var zipStream = await storageFile.OpenStreamForWriteAsync();
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
        foreach (var filePath in filePaths)
        {
            // You need to get the file as a StorageFile
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            var fileName = file.Name;
            var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);

            using var entryStream = entry.Open();
            // Open the file as a stream to copy it into the zip entry
            using var fileStream = await file.OpenStreamForReadAsync();
            await fileStream.CopyToAsync(entryStream);
        }
    }
}