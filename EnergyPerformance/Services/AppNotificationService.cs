using System.Collections.Specialized;
using System.Web;

using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using EnergyPerformance.Services;
using Microsoft.Windows.AppNotifications;

namespace EnergyPerformance.Notifications;

/// <summary>
/// Notification service.
/// </summary>
public class AppNotificationService : IAppNotificationService
{
    private readonly INavigationService _navigationService;
    private readonly AutoConfigurationService autoConfigurationService = new();

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
        var notificationManager = AppNotificationManager.Default;
        notificationManager.NotificationInvoked += OnNotificationInvoked;
        notificationManager.Register();
    }

    /// <summary>
    /// Invoked when a notification is activated by the user.
    /// <param name="sender"></param>
    /// <param name="args">Contains the custom action of a toast notification and may also include the persona name.</param>
    /// </summary>
    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        // Can perform custom actions here based on arguments specified in notification payload.
        // E.g. a button within the notification can be used to perform a specific action which can be defined here.
        App.MainWindow.DispatcherQueue.TryEnqueue( delegate
        {
            // Extract custom action from argument
            var customAction = args.Argument.Split('=')[1];
            var executablePath = "";

            // If executable path is included in the agrument
            // then divide custom action and executable path
            if (args.Argument.Contains('&'))
            {
                // Extract executable path
                executablePath = customAction.Split('&')[1];

                // Extract the custom action
                customAction = customAction.Split('&')[0];
            }

            switch (customAction)
            {
                case "enableLaunchedAppPersona":
                    App.GetService<PersonaModel>().EnablePersona(executablePath);
                    
                    break;
                case "autoConfigurePersona":
                    PersonaNotification.AutoConfigurePersonaNotification(executablePath);

                    break;
                case "startAutoConfiguration":
                    PersonaNotification.StartAutoConfigurationNotification(executablePath);
                    autoConfigurationService.Start(executablePath);

                    break;
                case "moveTowardsPerformance":
                    autoConfigurationService.AutoConfigure(true);

                    break;
                case "moveTowardsEfficiency":
                    autoConfigurationService.AutoConfigure(false);

                    break;
            }
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
