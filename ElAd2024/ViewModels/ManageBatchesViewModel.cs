using ElAd2024.Contracts.Services;
using ElAd2024.Models.Database;

namespace ElAd2024.ViewModels;

public partial class ManageBatchesViewModel(IDatabaseService databaseService) : BaseManageViewModel<Batch>(databaseService, databaseService.Batches)
{
    protected override bool EnsureCanDelete() => Selected is not null;


}