using Microsoft.Toolkit.Uwp.Notifications;

namespace EnergyPerformance.Helpers;

/// <summary>
/// Notification specific to Personas
/// </summary>
internal class PersonaNotification
{
    /// <summary>
    /// Notifies the user to enable Persona if needed.
    /// <param name="executable">The executable application.</param>
    /// </summary>
    public static void EnablePersonaNotification(string executable)
    {
        new ToastContentBuilder()
            .AddText($"Enable Persona for {executable.ToLower()}?")
            .AddButton(new ToastButton()
                .SetContent("Enable")
                .AddArgument("action", $"autoConfigurePersona&{executable}"))
            .AddButton(new ToastButtonDismiss("Dismiss"))
            .Show();
    }

    /// <summary>
    /// Notifies the user to auto-configure Persona if needed.
    /// <param name="executable">The executable application.</param>
    /// </summary>
    public static void AutoConfigurePersonaNotification(string executable)
    {
        new ToastContentBuilder()
            .AddText($"Auto-configure Persona for {executable.ToLower()}?")
            .AddButton(new ToastButton()
                .SetContent("Yes")
                .AddArgument("action", $"startAutoConfiguration&{executable}"))
            .AddButton(new ToastButton()
                .SetContent("No")
                .AddArgument("action", $"enableLaunchedAppPersona&{executable}"))
            .Show();
    }


    /// <summary>
    /// Notifies the user that the auto-configuration has started.
    /// <param name="executable">The executable application.</param>
    /// </summary>
    public static void StartAutoConfigurationNotification(string executable)
    {
        new ToastContentBuilder()
            .AddText($"Auto-configuration of Persona for {executable.ToLower()} has started.\n\n" +
                $"We'll inquire about the application's smoothness five (5) times every three (3) minutes during usage.")
            .AddButton(new ToastButtonDismiss("Dismiss"))
            .Show();
    }

    /// <summary>
    /// Asks user if current application is running smoothly.
    /// <param name="executable">The executable application.</param>
    /// </summary>
    public static void SmoothnessCheckNotification(string executable, int smoothnessCheckCount)
    {
        new ToastContentBuilder()
            .AddText($"[{smoothnessCheckCount}/5] Is {executable.ToLower()} running smoothly?")
            .AddButton(new ToastButton()
                .SetContent("Yes")
                .AddArgument("action", $"moveTowardsPerformance&{executable}"))
            .AddButton(new ToastButton()
                .SetContent("No")
                .AddArgument("action", $"moveTowardsEfficiency&{executable}"))
            .Show();
    }

    public static void AutoConfigurationSuccessNotification(string executable)
    {
        new ToastContentBuilder()
            .AddText($"Successfully auto-configured Persona for {executable.ToLower()}.")
            .AddButton(new ToastButtonDismiss("Dismiss"))
            .Show();
    }

    /// <summary>
    /// Notifies the user that the Persona has been disabled.
    /// <param name="executablePath">Path to the program's executable.</param>
    /// </summary>
    public static void DisabledPersonaNotification(string executable)
    {
        new ToastContentBuilder()
            .AddText($"Disabled Persona for {executable.ToLower()}")
            .Show();
    }

    public static void FailedAutoConfigurationNotification(string executable)
    {
        new ToastContentBuilder()
            .AddText($"Unable to auto-configure Persona for {executable.ToLower()}.\n\n" +
                "Want to try again?")
            .AddButton(new ToastButton()
                .SetContent("Retry")
                .AddArgument("action", $"startAutoConfiguration&{executable}"))
            .AddButton(new ToastButtonDismiss("Dismiss"))
            .Show();
    }
}
