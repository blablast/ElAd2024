using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElAd2024.Contracts.Services;
using ElAd2024.Views.Dialogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Telerik.UI.Xaml.Controls.Grid;

namespace ElAd2024.ViewModels;
public abstract partial class BaseManageViewModel<T> : ObservableRecipient, IDisposable where T : class, new()
{
    private readonly IDatabaseService db;
    private readonly DbSet<T> dbSet;

    [NotifyCanExecuteChangedForAttribute(nameof(DeleteCommand))]
    [ObservableProperty] private bool canDelete;
    
    [ObservableProperty] private bool exists;
    [ObservableProperty] private T? selected;
    [ObservableProperty] private bool isSelected;

    partial void OnSelectedChanged(T? value) => SelectedChanged(value);
    protected virtual void SelectedChanged(T? value) {
        Debug.WriteLine($"SelectedChanged in base class!"); }

    public ObservableCollection<T> Items { get; set; }

    public BaseManageViewModel(IDatabaseService databaseService, DbSet<T> dbSet)
    {
        db = databaseService;
        this.dbSet = dbSet;

        Items = new ObservableCollection<T>(dbSet);
        Exists = Items.Any();
    }

    public virtual void Dispose() => GC.SuppressFinalize(this);

    protected virtual bool EnsureCanDelete() => true;
    protected virtual void OnNewAdded(T newItem) { }

    [RelayCommand]
    private async Task Add()
    {
        var newItem = new T();
        dbSet.Add(newItem);
        if (await db.Context.SaveChangesAsync() == 0)
        {
            throw new InvalidOperationException("Failed to add new item to database!");
        }
        Items.Add(newItem);
        OnNewAdded(newItem);

        Selected = newItem;
        IsSelected = true;
        Exists = Items.Any();
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task Delete()
    {
        if (Selected is not null
            && await Dialogs.ShowYesNoQuestionAsync("Delete", "Do you want to delete selected?"))
        {
            dbSet.Remove(Selected);
            await db.Context.SaveChangesAsync();
            Items.Remove(Selected);
            Exists = Items.Any();
            IsSelected = false;
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
            Selected = (T)cellInfo.Item;
            CanDelete = Selected is not null && EnsureCanDelete();
        }

        await Task.CompletedTask;
    }

}
