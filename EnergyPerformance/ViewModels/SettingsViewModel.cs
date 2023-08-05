using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;


namespace EnergyPerformance.ViewModels;

/// <summary>
/// ViewModel for the settings page.
/// </summary>
public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly CpuInfo _cpuInfo;
    private readonly ILocalSettingsService _settingsService;
    private const string DefaultAppDisplayName = "EnergyGuard";
    private const string AutoControlSettingKey = "AutoControl";
    private ElementTheme _elementTheme;
    private string _versionDescription;
    private readonly EnergyUsageModel _model;

    [ObservableProperty]
    private static bool enabledLaunchOnStartup;
    
    [ObservableProperty]
    private bool enableAutoControlToggle;

    /// <summary>
    /// Gets the auto control setting from local settings.
    /// Notifies all subscribers to the AutoControl property when the setting is changed.
    /// </summary>
    public bool AutoControl
    {
        get => _settingsService.AutoControlSetting;
        set
        {
            _settingsService.AutoControlSetting = value;
            OnPropertyChanged(nameof(AutoControl));
        }
    }

    /// <summary>
    /// Gets value from <see cref="CpuInfo"/> indicating whether the current device's CPU supports automatic control.
    /// </summary>
    public bool SupportedCpu => _cpuInfo.IsSupported;

    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public ICommand LaunchOnStartupCommand
    {
        get;
    }

    public ICommand SwitchAutoControlSettingCommand
    {
        get;
    }

    public bool IsLiveCost
    {
        get => _model.IsLiveCost;
        set => _model.IsLiveCost = value;
    }

    public double WeeklyBudget
    {
        get => _model.WeeklyBudget;
        set
        {
            if (double.IsNaN(value) || value <= 0)
            {
                // update numberbox again incase the 'X' button was pressed to make the value reappear
                OnPropertyChanged(nameof(WeeklyBudget));
                return;
            }

            _model.WeeklyBudget = value;
            OnPropertyChanged(nameof(WeeklyBudget));
        }
    }

    public double CostPerKwh
    {
        get => _model.CostPerKwh;
        set
        {
            if (double.IsNaN(value) || value <= 0)
            {
                // update numberbox again incase the 'X' button was pressed to make the value reappear
                OnPropertyChanged(nameof(CostPerKwh));
                return;
            }

            _model.CostPerKwh = value;
            OnPropertyChanged(nameof(CostPerKwh));
        }
    }


    public SettingsViewModel(IThemeSelectorService themeSelectorService, CpuInfo cpuInfo, ILocalSettingsService settingsService, EnergyUsageModel model)
    {
        _model = model;
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        EnableAutoControlToggle = true;
        _cpuInfo = cpuInfo;
        _versionDescription = GetVersionDescription();
        _settingsService = settingsService;
        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });

        LaunchOnStartupCommand = new RelayCommand<bool>(async (param) => await LaunchOnStartupActions(param));
        SwitchAutoControlSettingCommand = new RelayCommand<bool>(async (param) => await SwitchAutoControlSetting(param, _cpuInfo.IsSupported));

    }

    /// <summary>
    /// Performs initialization tasks for the settings page.
    /// Includes retrieving the auto control setting from local settings and whether the current app is enabled/disabled for launching on startup.
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
        AutoControl = await LoadAutoControlSetting(_cpuInfo.IsSupported);
        // the live setting from Windows is retrieved for the most up-to-date and correct value, rather than reading the setting from local settings
        EnabledLaunchOnStartup = await GetStartupState(); 
    }

    /// <summary>
    /// Loads the auto control setting from local settings.
    /// </summary>
    /// <param name="isSupported">Whether the current device is supported for hybrid core shift operations.</param>
    /// <returns></returns>
    private async Task<bool> LoadAutoControlSetting(bool isSupported)
    {
        if (!isSupported)
        {
            await _settingsService.SaveSettingAsync(AutoControlSettingKey, false);
            EnableAutoControlToggle = false;
            return false;
        }

        EnableAutoControlToggle = true;
        var setting = await _settingsService.ReadSettingAsync<bool>(AutoControlSettingKey);
        AutoControl = setting;
        return setting;
    }

    /// <summary>
    /// Switches the current state of the auto control setting and saves it to local settings.
    /// No action is taken if the setting is not supported for the device (i.e. below 12th gen Intel CPU).
    /// </summary>
    private async Task SwitchAutoControlSetting(bool setting, bool isSupported)
    {
        if (!isSupported)
        {
            return;
        }
        AutoControl = setting;
        await _settingsService.SaveSettingAsync(AutoControlSettingKey, setting);
    }

    /// <summary>
    /// Retrieves the current state of the startup task for the appication.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> GetStartupState()
    {
        var startupTask = await StartupTask.GetAsync("EnergyGuard");
        return startupTask.State == StartupTaskState.Enabled;
    }

    /// <summary>
    /// Retrieves the current state of the startup task for the appication and enables/disables it accordingly based on the argument.
    /// </summary>
    /// <param name="enable">Bool representing whether to enable (true) or disable (false) launching on startup for the app.</param>
    /// <returns></returns>
    public async Task LaunchOnStartupActions(bool enable)
    {
        var startupTask = await StartupTask.GetAsync("EnergyGuard");
        var startupState = startupTask.State;
        var requestResult = startupTask.State;
        if (!enable)
        {
            if (startupState.Equals(StartupTaskState.Enabled))
            {
                Debug.WriteLine("Disabling Startup Task");
                startupTask.Disable();
                requestResult = startupTask.State;
            }
            Debug.WriteLine("Result: " + requestResult);
            EnabledLaunchOnStartup = requestResult == StartupTaskState.Enabled;
            return;
        }
        switch (startupState)
        {
            // print statements for debugging, no action is taken if startup task cannot be enabled
            // the toggle switch is turned off in the frontend and the user can re-attempt to enable this at a later date.
            case StartupTaskState.Disabled:
                // Task is disabled but can be enabled.
                StartupTaskState newState = await startupTask.RequestEnableAsync();
                requestResult = newState;
                Debug.WriteLine("Request to enable startup, result = {0}", requestResult.ToString());
                break;
            case StartupTaskState.DisabledByUser:
                // Task is disabled and user must enable it manually.
                Debug.WriteLine("Task is disabled and user must enable it manually.");
                break;
            case StartupTaskState.DisabledByPolicy:
                Debug.WriteLine(
                    "Startup disabled by group policy, or not supported on this device");
                break;
            case StartupTaskState.Enabled:
                Debug.WriteLine("Startup is already enabled.");
                break;
        }
        Debug.WriteLine("Result: " + requestResult.ToString());
        EnabledLaunchOnStartup = requestResult == StartupTaskState.Enabled;
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        string localizedDisplayName;
        try
        {
            localizedDisplayName = "AppDisplayName".GetLocalized();
        } catch
        {
            localizedDisplayName = DefaultAppDisplayName;
        }
        return $"{localizedDisplayName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

}
