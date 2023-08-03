using Microsoft.Toolkit.Uwp.Notifications;

namespace EnergyPerformance.Services;

/// <summary>
/// Notification specific to Personas
/// </summary>
internal class PersonaNotificationService
{
    /// <summary>
    /// Notifies the user to enable Persona if desired.
    /// <param name="executablePath">Path to the program's executable.</param>
    /// </summary>
    public static void EnablePersona(string executablePath)
    {
        new ToastContentBuilder()
            .AddText($"Enable Persona for {executablePath}?")
            .AddButton(new ToastButton()
                .SetContent("Enable")
                .AddArgument("action", $"autoConfigurePersonaNotification&{executablePath}"))
            .AddButton(new ToastButtonDismiss())
            .Show();
    }

    /// <summary>
    /// Notifies the user to auto-configure Persona if desired.
    /// <param name="executablePath">Path to the program's executable.</param>
    /// </summary>
    public static void AutoConfigurePersona(string executablePath)
    {
        new ToastContentBuilder()
            .AddText($"Auto-configure Persona for {executablePath}?")
            .AddButton(new ToastButton()
                .SetContent("Yes")
                .AddArgument("action", $"chooseConfigurationNotification&{executablePath}"))
            .AddButton(new ToastButton()
                .SetContent("No")
                .AddArgument("action", $"enableLaunchedAppPersona&{executablePath}"))
            .Show();
    }

    /// <summary>
    /// Notifies the user to choose from Persona configuration options.
    /// <param name="executablePath">Path to the program's executable.</param>
    /// </summary>
    public static void ChooseConfiguration(string executablePath)
    {
        new ToastContentBuilder()
            .AddText($"Choose Persona configuration for {executablePath}:")
            .AddButton(new ToastButton()
                .SetContent("Performant")
                .AddArgument("action", $"configurPersonaToPerformant&{executablePath}"))
            .AddButton(new ToastButton()
                .SetContent("Balanced")
                .AddArgument("action", $"configurPersonaToBalanced&{executablePath}"))
            .AddButton(new ToastButton()
                .SetContent("Efficient")
                .AddArgument("action", $"configurPersonaToEfficient&{executablePath}"))
            .Show();
    }

    /// <summary>
    /// Notifies the user that the Persona has been disabled.
    /// <param name="executablePath">Path to the program's executable.</param>
    /// </summary>
    public static void DisabledPersona(string executablePath)
    {
        new ToastContentBuilder()
            .AddText($"Disabled Persona for {executablePath}")
            .Show();
    }
}
