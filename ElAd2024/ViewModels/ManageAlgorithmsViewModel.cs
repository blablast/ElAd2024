using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using ElAd2024.Contracts.Services;
using ElAd2024.Helpers;
using ElAd2024.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ElAd2024.ViewModels;

public partial class ManageAlgorithmsViewModel : BaseManageViewModel<Algorithm>
{
    private readonly IDatabaseService db;
    private ObservableCollection<Algorithm> allAlgorithms;
    public ObservableCollectionNotifyPropertyChange<AlgorithmStepViewModel> SelectedAlgorithmSteps { get; set; } = [];

    public ObservableCollection<AlgorithmStepViewModel> AvailableAlgorithmSteps { get; set; } = [];


    public ManageAlgorithmsViewModel(IDatabaseService databaseService) : base(databaseService, databaseService.Algorithms)
    {
        db = databaseService;

        allAlgorithms = new ObservableCollection<Algorithm>(db.Algorithms.Include(a => a.AlgorithmSteps).ThenInclude(s => s.Step));
        var availableSteps = db.Steps.Where(s => s.IsMoveable == true).ToList();
        availableSteps.ForEach(step => AvailableAlgorithmSteps.Add(new AlgorithmStepViewModel(GetNew(step))));
    }

    private async void OnSelectedAlgorithmStepsDataChanged(object? sender, PropertyChangedEventArgs e)
        => await Save(null);


    public void AddAvailableStep(int stepId, int index)
    {
        var step = db.Steps.Single(s => s.Id == stepId);

        if (index >= SelectedAlgorithmSteps.Count)
        {
            SelectedAlgorithmSteps.Add(new AlgorithmStepViewModel(GetNew(step)));
        }
        else
        {
            SelectedAlgorithmSteps.Insert(index, new AlgorithmStepViewModel(GetNew(step)));
        }
        Reorder();
    }

    private static AlgorithmStep GetNew(Step step)
        => new()
        {
            Step = step,
            FrontName = step.AsyncActionName,
            BackName = "Running...",
            ActionParameter = "<Value>"
        };

    protected async override void OnNewAdded(Algorithm newItem)
    {
        base.OnNewAdded(newItem);
        var mandatorySteps = db.Steps.Where(s => s.IsMandatory == true).ToList();
        var record = db.Algorithms.Single(a => a.Id == newItem.Id);
        mandatorySteps.ForEach(step => record.AlgorithmSteps.Add(GetNew(step)));
        newItem.Name = $"Algorithm #{allAlgorithms.Count}";
        await db.Context.SaveChangesAsync();

        allAlgorithms = new ObservableCollection<Algorithm>(db.Algorithms.Include(a => a.AlgorithmSteps).ThenInclude(s => s.Step));
    }

    // Properties changed methods
    protected override void SelectedChanged(Algorithm? value)
    {
        base.SelectedChanged(value);
        SelectedAlgorithmSteps?.Stop();

        if (value is not null)
        {
            SelectedAlgorithmSteps!.Clear();
            var current = allAlgorithms.Single(a => a.Id == value.Id).AlgorithmSteps.OrderBy(a => a.Order);

            foreach (var step in current)
            {
                SelectedAlgorithmSteps.Add(new AlgorithmStepViewModel(step));
            }
            Reorder();
            IsSelected = true;
            SelectedAlgorithmSteps.Start(OnSelectedAlgorithmStepsDataChanged);
        }

        CanDelete = EnsureCanDelete();
    }
    protected override bool EnsureCanDelete() => Selected?.Id > 1;

    public void Reorder()
    {
        if (SelectedAlgorithmSteps.Count < 2)
        {
            return;
        }

        var first = SelectedAlgorithmSteps.Single(s => s.AlgorithmStep!.Step.IsFirst == true);
        SelectedAlgorithmSteps.Remove(first);
        SelectedAlgorithmSteps.Insert(0, first);

        var last = SelectedAlgorithmSteps.Single(s => s.AlgorithmStep!.Step.IsLast == true);
        SelectedAlgorithmSteps.Remove(last);
        SelectedAlgorithmSteps.Add(last);

        for (var i = 0; i < SelectedAlgorithmSteps.Count; i++)
        {
            SelectedAlgorithmSteps[i].Order = i;
        }

    }

    [RelayCommand]
    public async Task Save(object? param)
    {
        if (Selected is null) { return; }
        var selectedId = Selected.Id;
        var record = db.Algorithms.Single(a => a.Id == selectedId);
        record.AlgorithmSteps.Clear();

        foreach (var step in SelectedAlgorithmSteps)
        {
            record.AlgorithmSteps.Add(new AlgorithmStep
            {
                StepId = step.AlgorithmStep!.StepId,
                Step = step.AlgorithmStep!.Step,
                FrontName = step.FrontName,
                BackName = step.BackName,
                ActionParameter = step.ActionParameter ?? string.Empty,
                Order = step.Order
            });
        }

        await db.Context.SaveChangesAsync();
        allAlgorithms = new ObservableCollection<Algorithm>(db.Algorithms.Include(a => a.AlgorithmSteps).ThenInclude(s => s.Step));
        Selected = allAlgorithms.Single(a => a.Id == selectedId);

        Debug.WriteLine("Alghorithm Saved");
    }

}
