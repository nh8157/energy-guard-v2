// TemplateStudio

using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;

using Microsoft.UI.Xaml;

namespace EnergyPerformance.Services;

/// <summary>
/// Class responsible for loading and saving user's preferred theme in settings, as well as
/// applying it to the application at runtime.
/// </summary>
public class ThemeSelectorService : IThemeSelectorService
{
    private const string SettingsKey = "AppBackgroundRequestedTheme";

    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    private readonly ILocalSettingsService _localSettingsService;

    public ThemeSelectorService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        Theme = await LoadThemeFromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task SetThemeAsync(ElementTheme theme)
    {
        Theme = theme;

        await SetRequestedThemeAsync();
        await SaveThemeInSettingsAsync(Theme);
    }

    /// <summary>
    /// Sets the requested theme to the <see cref="MainWindow"/>.
    /// </summary>
    /// <returns></returns>
    public async Task SetRequestedThemeAsync()
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = Theme;

            TitleBarHelper.UpdateTitleBar(Theme);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Loads the application theme from settings.
    /// </summary>
    /// <returns></returns>
    private async Task<ElementTheme> LoadThemeFromSettingsAsync()
    {
        var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);

        if (Enum.TryParse(themeName, out ElementTheme cacheTheme))
        {
            return cacheTheme;
        }

        return ElementTheme.Default;
    }

    /// <summary>
    /// Saves the application theme in settings.
    /// </summary>
    /// <param name="theme"></param>
    /// <returns></returns>
    private async Task SaveThemeInSettingsAsync(ElementTheme theme)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, theme.ToString());
    }
}
