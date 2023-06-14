using System.ComponentModel;
using System.Diagnostics;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Contracts.Services;
using EnergyPerformance.Core.Helpers;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;

using Microsoft.Extensions.Options;

using Windows.Storage;

namespace EnergyPerformance.Services;

/// <summary>
/// Service that provides access to local settings and acts as a container for getting and setting
/// user's settings choices for other classes (Services, ViewModels and Models).
/// </summary>
public class LocalSettingsService : ILocalSettingsService
{
    private const string _defaultApplicationDataFolder = "EnergyPerformance/ApplicationData";
    private const string _defaultLocalSettingsFile = "LocalSettings.json";
    private const string _defaultSelectedMode = "Auto";

    private readonly IFileService _fileService;
    private readonly LocalSettingsOptions _options;

    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string _applicationDataFolder;
    private readonly string _localsettingsFile;
    public event PropertyChangedEventHandler? AutoControlEventHandler;
    private bool autoControlSetting;
    private string selectedMode;


    /// <summary>
    /// Retrieves the currently set setting for automatic control of the CPU.
    /// </summary>
    public bool AutoControlSetting
    {
        get => autoControlSetting;
        set
        {
            autoControlSetting = value;
            if (AutoControlEventHandler != null)
            {
                AutoControlEventHandler(this, new PropertyChangedEventArgs(nameof(AutoControlSetting)));
            }

        }
    }

    /// <summary>
    /// Retrieves the currently set energy profile mode in the home page.
    /// </summary>
    public string SelectedMode
    {
        get => selectedMode;
        set => selectedMode = value;
    }


    private IDictionary<string, object> _settings;

    private bool _isInitialized;


    public LocalSettingsService(IFileService fileService, IOptions<LocalSettingsOptions> options)
    {
        _fileService = fileService;
        _options = options.Value;
        selectedMode = _defaultSelectedMode;
        _applicationDataFolder = Path.Combine(_localApplicationData, _options.ApplicationDataFolder ?? _defaultApplicationDataFolder);
        ; _localsettingsFile = _options.LocalSettingsFile ?? _defaultLocalSettingsFile;
        AutoControlSetting = false;
        _settings = new Dictionary<string, object>();
    }

    /// <summary>
    /// Performs asynchronous operations required on startup. 
    /// Reads the local settings file.
    /// </summary>
    /// <returns></returns>
    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _settings = await Task.Run(() => _fileService.Read<IDictionary<string, object>>(_applicationDataFolder, _localsettingsFile)) ?? new Dictionary<string, object>();

            _isInitialized = true;
        }
    }


    // TemplateStudio
    /// <summary>
    /// Reads a setting from the local settings file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">String key to the JSON file</param>
    /// <returns>The value stored in JSON if found, otherwise returns a default value for the object</returns>
    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        if (RuntimeHelper.IsMSIX)
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
        }
        else
        {
            await InitializeAsync();

            if (_settings != null && _settings.TryGetValue(key, out var obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
        }

        return default;
    }

    // TemplateStudio
    /// <summary>
    /// Saves a setting to the local settings file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">String key to the JSON file</param>
    /// <param name="value">The value requested to be saved in JSON</param>
    /// <returns></returns>
    public async Task SaveSettingAsync<T>(string key, T value)
    {
        if (RuntimeHelper.IsMSIX)
        {
            ApplicationData.Current.LocalSettings.Values[key] = await Json.StringifyAsync(value);
        }
        else
        {
            await InitializeAsync();

            _settings[key] = await Json.StringifyAsync(value);

            await Task.Run(() => _fileService.Save(_applicationDataFolder, _localsettingsFile, _settings));
        }
    }
}
