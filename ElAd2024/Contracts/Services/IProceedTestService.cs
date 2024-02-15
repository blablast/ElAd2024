using ElAd2024.Contracts.Devices;
using ElAd2024.Helpers;
using ElAd2024.Models;
using ElAd2024.Models.Database;
using ElAd2024.ViewModels;

namespace ElAd2024.Contracts.Services;
public interface IProceedTestService : IDisposable
{
    public enum ErrorType
    {
        None,
        WeightIsZero
    }
    ObservableCollectionNotifyPropertyChange<AlgorithmStepViewModel> AlgorithmSteps { get; set;}
    int CurrentStep { get; set; }

    ErrorType Error { get; set; }
    bool IsRunning { get; set; }

    IAllDevices AllDevices { get; set; }
    TestParameters? Parameters { get; set; }
    
    Test CurrentTest { get; set; }
    Task StartTest();
    Task InitializeStepsAsync(int algorithmId);
}