using System.ComponentModel;

namespace EnergyPerformance.Contracts.Services;

public interface ILocalSettingsService
{
    bool AutoControlSetting
    {
        get;
        set;
    }
    string SelectedMode
    {
        get;
        set;
    }

    event PropertyChangedEventHandler AutoControlEventHandler;

    Task<T?> ReadSettingAsync<T>(string key);

    Task SaveSettingAsync<T>(string key, T value);
}
