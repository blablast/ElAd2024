
namespace ElAd2024.Contracts.Services;
public interface IDialogService
{
    Task<bool> ShowYesNoDialogAsync(string title, string content);
}