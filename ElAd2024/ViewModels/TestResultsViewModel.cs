using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Services;
using ElAd2024.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ElAd2024.ViewModels;

public partial class TestResultsViewModel(IDatabaseService databaseService) : ObservableRecipient
{
    [ObservableProperty] private Batch? selected;
    [ObservableProperty] private Test? selectedTest;

    public ObservableCollection<Batch> Batches => new(
        databaseService
            .Batches
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Photos)
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Voltages)
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Weights)
            );
}