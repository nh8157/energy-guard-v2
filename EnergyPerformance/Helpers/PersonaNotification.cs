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
            .AddText($"Persona auto-configuration for {executable} has started.\n\n" +
                "We'll inquire about app stability during usage.")
            .AddButton(new ToastButtonDismiss("Dismiss"))
            .Show();
    }

    /// <summary>
    /// Asks user if current application is stable.
    /// <param name="executable">The executable application.</param>
    /// </summary>
    public static void StabilityCheckNotification(string executable)
    {
        new ToastContentBuilder()
            .AddText($"Is the performance of {executable} stable?")
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
            .AddText($"Successfully auto-configured Persona for {executable}.")
            .AddButton(new ToastButtonDismiss("Dismiss"))
            .Show();
    }

    /// <summary>
    /// Notifies the user that the Persona has been disabled.
    /// <param name="executablePath">Path to the program's executable.</param>
    /// </summary>
    public static void DisabledPersonaNotification(string executablePath)
    {
        new ToastContentBuilder()
            .AddText($"Disabled Persona for {executablePath.ToLower()}")
            .Show();
    }

    public static void FailedAutoConfigurationNotification(string executablePath)
    {
        new ToastContentBuilder()
            .AddText($"Unable to auto-configure Persona for {executablePath.ToLower()}.\n\n" +
                "Please try manual configuration.")
            .AddButton(new ToastButtonDismiss("Dismiss"))
            .Show();
    }
}
