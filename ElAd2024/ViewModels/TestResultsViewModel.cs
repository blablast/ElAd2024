using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElAd2024.Contracts.Services;
using ElAd2024.Helpers;
using ElAd2024.Models.Database;
using ElAd2024.Views.Dialogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Windows.Storage;

namespace ElAd2024.ViewModels;

public partial class TestResultsViewModel(IDatabaseService databaseService, ILocalSettingsService localSettingsService) : ObservableRecipient
{
    [ObservableProperty] private Batch? selected;
    [ObservableProperty] private Test? selectedTest;

    public readonly ObservableCollection<Batch> Batches = new(
        databaseService
            .Batches
            .Include(b => b.Tests).ThenInclude(t => t.Photos)
            .Include(b => b.Tests).ThenInclude(t => t.Voltages)
            .Include(b => b.Tests).ThenInclude(t => t.Weights)
            .Include(b => b.Tests).ThenInclude(t => t.Humidities)
            .Include(b => b.Tests).ThenInclude(t => t.Temperatures)
            .Include(b => b.Tests).ThenInclude(t => t.ElectroStatics)   
            );

    [RelayCommand]
    public async Task ZipFiles()
    {
        var dbFile = await StorageFile.GetFileFromPathAsync(databaseService.DbPath);
        var settingsFile = await StorageFile.GetFileFromPathAsync(Path.Combine(localSettingsService.ApplicationDataFolder, localSettingsService.LocalSettingsFile));
        List<string> files = [dbFile.Path, settingsFile.Path];
        var photos = Batches.SelectMany(batch => batch.Tests.SelectMany(test => test.Photos))
                            .Select(photo => photo.ImageSource)
                            .ToList();
        files.AddRange(photos);
        var fileName = $"ElAd2024_{DateTime.Now:yyyyMMddHHmmss}.zip";
        await ZipFilesHelper.CreateZipFileAsync(files, fileName);
        await Dialogs.ShowInfoAsync("Info", $"ZIP Archive created and saved in Documents Folder: {fileName}");
        var documentsFolder = KnownFolders.DocumentsLibrary;
        var storageFile = await documentsFolder.GetFileAsync(fileName);

        await FilesAndFolders.OpenFolderAsync(storageFile.Path);
    }
}