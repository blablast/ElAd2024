using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Services;
using ElAd2024.Models;
using Microsoft.EntityFrameworkCore;

namespace ElAd2024.ViewModels;

public partial class TestResultsViewModel : ObservableRecipient
{
    [ObservableProperty] private Batch? selected;

    [ObservableProperty] private Test? selectedTest;


    partial void OnSelectedChanged(Batch? value) 
    {
    }
    public ObservableCollection<Batch> Batches
    {
        get; set;
    }

    public ObservableCollection<Batch> ChartSeries
    {
        get; set;
    } = new();


    // Dependency Injections
    private readonly IDatabaseService db;
    private readonly ILocalSettingsService localSettingsService;

    public TestResultsViewModel(ILocalSettingsService localSettingsService, IDatabaseService databaseService)
    {
        db = databaseService;

        this.localSettingsService = localSettingsService;
        Batches = new ObservableCollection<Batch>(db
            .Batches
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Photos)
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Voltages)
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Weights)

            );
    }

}