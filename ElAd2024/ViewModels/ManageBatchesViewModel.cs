using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElAd2024.Contracts.Services;
using Telerik.UI.Xaml.Controls.Grid;
using ElAd2024.Models;
using ElAd2024.Services;
using Microsoft.UI.Xaml;
using ElAd2024.Views;

namespace ElAd2024.ViewModels;

public partial class ManageBatchesViewModel : ObservableRecipient, IDisposable
{
    private readonly IDatabaseService db;
    private Batch? selected;
    private XamlRoot? xamlRoot;

    [NotifyCanExecuteChangedForAttribute(nameof(DeleteCommand))]
    [ObservableProperty] private bool canDelete = false;

    [ObservableProperty] private bool exists;
    public ObservableCollection<Batch> Batches { get; set; }


    public ManageBatchesViewModel( IDatabaseService databaseService)
    {
        db = databaseService;
        Batches = new(db.Batches);
        Exists = Batches.Any();
    }

    public async Task InitializeAsync(XamlRoot xamlRoot)
    {
        this.xamlRoot = xamlRoot;
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task Add()
    {
        var newBatch = new Batch();
        db.Batches.Add(newBatch);
        if (await db.Context.SaveChangesAsync() == 0)
        {
            throw new InvalidOperationException("Failed to add new test to database!");
        }
        Batches.Add(newBatch);
        Exists = Batches.Any();
    }

    [RelayCommand(CanExecute=nameof(CanDelete))]
    private async Task Delete()
    {
        if (selected is not null
            && xamlRoot is not null
            && await CustomContentDialog.ShowYesNoQuestionAsync(xamlRoot, "Delete", "Do you want to delete selected batch?"))
        {
            db.Batches.Remove(selected);
            await db.Context.SaveChangesAsync();
            Batches.Remove(selected);
            Exists = Batches.Any();
            CanDelete = false;
        }
    }

    [RelayCommand]
    private async Task CommitEdit(object param)
        => await db.Context.SaveChangesAsync();

    [RelayCommand]
    private async Task CellTap(object param)
    {
        if (param is DataGridCellInfo cellInfo)
        {
            selected = (Batch)cellInfo.Item;
            CanDelete = selected is not null;
        }
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
