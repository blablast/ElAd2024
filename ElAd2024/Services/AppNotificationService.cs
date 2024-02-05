using System.Collections.Specialized;
using System.Web;

using ElAd2024.Contracts.Services;
using ElAd2024.ViewModels;

using Microsoft.Windows.AppNotifications;

namespace ElAd2024.Notifications;

public class AppNotificationService(INavigationService navigationService) : IAppNotificationService
{
    private readonly INavigationService navigationService = navigationService;

    ~AppNotificationService()
    {
        Unregister();
    }

    public void Initialize()
    {
        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
        AppNotificationManager.Default.Register();
    }

    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        // TODO: Handle notification invocations when your app is already running.

        // Navigate to a specific page based on the notification arguments.
        if (ParseArguments(args.Argument)["action"] == "Settings")
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
            });
        }

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            App.MainWindow.ShowMessageDialogAsync("TODO: Handle notification invocations when your app is already running.", "Notification Invoked");
            App.MainWindow.BringToFront();
        });
    }

    public bool Show(string payload)
    {
        var appNotification = new AppNotification(payload);
        AppNotificationManager.Default.Show(appNotification);
        return appNotification.Id != 0;
    }

    public NameValueCollection ParseArguments(string arguments)
        => HttpUtility.ParseQueryString(arguments);

    public void Unregister()
        => AppNotificationManager.Default.Unregister();
}
