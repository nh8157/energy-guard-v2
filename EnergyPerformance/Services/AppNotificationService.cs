using System.Collections.Specialized;
using System.Web;

using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;

using Microsoft.Windows.AppNotifications;

namespace EnergyPerformance.Notifications;

/// <summary>
/// Service to show notifications to the user
/// </summary>
public class AppNotificationService : IAppNotificationService
{
    private readonly INavigationService _navigationService;

    public AppNotificationService(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    ~AppNotificationService()
    {
        Unregister();
    }

    public void Initialize()
    {
        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;

        AppNotificationManager.Default.Register();
    }

    /// <summary>
    /// Invoked when a notification is activated by the user.
    /// </summary>
    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        // Can perform custom actions here based on arguments specified in notification payload.
        // E.g. a button within the notification can be used to perform a specific action which can be defined here.
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            // Bring app to foreground.
            App.MainWindow.BringToFront();
        });
    }

    /// <summary>
    /// Shows a new notification.
    /// </summary>
    /// <param name="payload">The name of the notification payload defined in the Resources file.</param>
    public bool Show(string payload)
    {
        // Retrieve XAML payload from Resources.
        payload = string.Format(payload.GetLocalized(), AppContext.BaseDirectory);
        var appNotification = new AppNotification(payload);
        AppNotificationManager.Default.Show(appNotification);
        
        return appNotification.Id != 0;
    }

    /// <summary>
    /// Shows a new notification and removes all prior notifications sent by the app.
    /// Can be used to ensure this app only shows at most one notification at a time to prevent cluttering the notification tray.
    /// </summary>
    /// <param name="payload">The name of the notification payload defined in the Resources file.</param>
    public async Task ShowAsync(string payload)
    {
        // Retrieve XAML payload from Resources.
        payload = string.Format(payload.GetLocalized(), AppContext.BaseDirectory);
        var appNotification = new AppNotification(payload);
        await AppNotificationManager.Default.RemoveAllAsync();
        AppNotificationManager.Default.Show(appNotification);
    }


    public NameValueCollection ParseArguments(string arguments)
    {
        return HttpUtility.ParseQueryString(arguments);
    }

    public void Unregister()
    {
        AppNotificationManager.Default.Unregister();
    }
}
