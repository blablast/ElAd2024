using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Services;
using ElAd2024.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ElAd2024.ViewModels;

public partial class TestResultsViewModel : ObservableRecipient
{
    [ObservableProperty] private Batch? selected;
    [ObservableProperty] private Test? selectedTest;

    private readonly ObservableCollection<Batch> batches;

    public TestResultsViewModel(IDatabaseService databaseService)
    {
        batches = new(
        databaseService
            .Batches
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Photos)
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Voltages)
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Weights)
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Humidities)
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Temperatures)
            );
    }

    public ObservableCollection<Batch> Batches => batches;
}