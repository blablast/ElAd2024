using System.Collections.ObjectModel;
using ElAd2024.Contracts.Services;
using ElAd2024.Models.Database;
using Microsoft.EntityFrameworkCore;
using Windows.ApplicationModel.Appointments.AppointmentsProvider;

namespace ElAd2024.ViewModels;

public partial class ManageAlgorithmsViewModel : BaseManageViewModel<Algorithm>
{
    private readonly IDatabaseService db;
    private readonly ObservableCollection<Algorithm> allAlgorithms;
    public ObservableCollection<AlgorithmStepViewModel> SelectedAlgorithmSteps { get; set; } = [];
    public ObservableCollection<AlgorithmStepViewModel> AvailableAlgorithmSteps { get; set; } = [];


    public ManageAlgorithmsViewModel(IDatabaseService databaseService)
        : base(databaseService, databaseService.Algorithms)
    {
        db = databaseService;

        allAlgorithms = new ObservableCollection<Algorithm>(db.Algorithms.Include(a => a.AlgorithmSteps).ThenInclude(s => s.Step));

        var availableSteps = db.Steps.Where(s => s.IsMoveable == true).ToList();
        availableSteps.ForEach(step => AvailableAlgorithmSteps.Add(new AlgorithmStepViewModel(GetNew(step))));
    }

    public void AddAvailableStep(int stepId)
    {
        var step = db.Steps.Single(s => s.Id == stepId);
        SelectedAlgorithmSteps.Add(new AlgorithmStepViewModel(GetNew(step)));
        Reorder();
    }

    private AlgorithmStep GetNew(Step step)
        => new()
        {
            Step = step,
            FrontName = step.Name,
            BackName = "Running...",
            ActionParameter = "<Value>"
        };

    // Properties changed methods
    protected override void SelectedChanged(Algorithm? value)
    {
        base.SelectedChanged(value);
        if (value is not null)
        {
            SelectedAlgorithmSteps.Clear();
            var current = allAlgorithms.Single(a => a.Id == value.Id).AlgorithmSteps.OrderBy(a => a.Order);
            current.ToList().ForEach(step => SelectedAlgorithmSteps.Add(new AlgorithmStepViewModel(step)));
        }
    }
    protected override bool EnsureCanDelete() => Selected?.Id > 1;

    public void Reorder()
    {
        if (SelectedAlgorithmSteps.Count <= 2)
        {
            return;
        }

        if (!SelectedAlgorithmSteps[0].IsFirst)
        {
            var firstItem = SelectedAlgorithmSteps[0];
            SelectedAlgorithmSteps.RemoveAt(0);
            SelectedAlgorithmSteps.Insert(1, firstItem);
        }
        if (!SelectedAlgorithmSteps[^1].IsLast)
        {
            var lastItem = SelectedAlgorithmSteps[^1];
            SelectedAlgorithmSteps.RemoveAt(SelectedAlgorithmSteps.Count - 1);
            SelectedAlgorithmSteps.Insert(SelectedAlgorithmSteps.Count - 1, lastItem);
        }

        for (var i = 0; i < SelectedAlgorithmSteps.Count; i++)
        {
            SelectedAlgorithmSteps[i].Order = i;
        }

    }

}
