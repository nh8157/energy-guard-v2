using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using EnergyPerformance.Services;
using Microsoft.Extensions.Hosting;

namespace EnergyPerformance.ViewModels;

/// <summary>
/// View model responsible for the main application view.
/// </summary>
public partial class CarbonEmissionViewModel : ObservableRecipient
{

    [ObservableProperty]
    private double budgetUsedPercent;

    private readonly EnergyUsageModel _model;
    private readonly ILocalSettingsService _settingsService;
    private readonly CpuInfo _cpuInfo;
    private readonly GpuInfo _gpuInfo;

    private readonly PowerInfo _powerInfo;
    private readonly IAppNotificationService _notificationService;
    private readonly CarbonIntensityInfo _carbonIntensityInfo;

    // must be a supported CPU and and auto control setting must be enabled in order to allow profile switching
    public bool AutoControl => _settingsService.AutoControlSetting && _cpuInfo.IsSupported;

    public double CpuPower => _powerInfo.CpuPower * _carbonIntensityInfo.CarbonIntensity;
    public double GpuPower => _powerInfo.GpuPower * _carbonIntensityInfo.CarbonIntensity;
    public double Power => _powerInfo.Power;
    public double TotalIntensity => _powerInfo.Power * _carbonIntensityInfo.CarbonIntensity;

    /// <summary>
    /// Gets the cost for the current week from the model.
    /// </summary>
    public float CostThisWeek => _model.GetCostForCurrentWeek();

    /// <summary>
    /// Gets the cost for the previous week from the model.
    /// </summary>
    public float CostPreviousWeek => _model.GetCostForPreviousWeek();

    public float EmissionsThisWeek = 0;
    public float EmissionsPreviousWeek = 10;


    /// <summary>
    /// Gets the selected mode from the settings service.
    /// </summary>
    public string SelectedMode
    {
        get => _settingsService.SelectedMode;
        set => _settingsService.SelectedMode = value;
    }

    /// <summary>
    /// Constructor for the MainViewModel.
    /// Sets the hardware info containers, services and model.
    /// Attaches event handlers for the hardware info containers and services.
    /// </summary>
    public CarbonEmissionViewModel(PowerInfo powerInfo, CpuInfo cpu, GpuInfo gpu
    , ILocalSettingsService settingsService, IAppNotificationService notificationService, EnergyUsageModel model, CarbonIntensityInfo carbonIntensityInfo)
    {
        // set hardware info containers
        _powerInfo = powerInfo;
        _cpuInfo = cpu;
        _gpuInfo = gpu;
        
        // set services
        _settingsService = settingsService;
        _notificationService = notificationService;
        _carbonIntensityInfo = carbonIntensityInfo;

        // set model
        _model = model;
        
        // attach event handlers
        _powerInfo.PowerUsageChanged += Power_PropertyChanged;
        _powerInfo.CpuPowerUsageChanged += CpuPower_PropertyChanged;
        _powerInfo.GpuPowerUsageChanged += GpuPower_PropertyChanged;
        _settingsService.AutoControlEventHandler += AutoControl_PropertyChanged;

        // set percentage value for circular progress bar monitoring the current budget
        BudgetUsedPercent = (model.GetCostForCurrentWeek() / model.WeeklyBudget) * 100;
    }


    [RelayCommand]
    public async Task SelectAutoControl()
    {
        SelectedMode = "Auto";
        await _notificationService.ShowAsync("AutoControlEnabledNotification");
    }

    /// <summary>
    /// Relay Commands for selecting the different modes
    /// </summary>
    /// <returns></returns>

    [RelayCommand]
    public async Task SelectCasualMode()
    {
        SelectedMode = "Casual";
        await _notificationService.ShowAsync("CasualModeNotification");
    }

    [RelayCommand]
    public async Task SelectWorkMode()
    {
        SelectedMode = "Work";
        await _notificationService.ShowAsync("WorkModeNotification");
    }


    [RelayCommand]
    public async Task SelectPerformanceMode()
    {
        SelectedMode = "Performance";
        await _notificationService.ShowAsync("PerformanceModeNotification");
    }


    /// <summary>
    /// Updates the power property when the power usage changes.
    /// </summary>
    private void Power_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_powerInfo.Power))
        {
            OnPropertyChanged(nameof(Power));
            OnPropertyChanged(nameof(TotalIntensity));
        }
        return;
    }

    /// <summary>
    /// Updates the auto control property when the auto control setting changes.
    /// </summary>
    private void AutoControl_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.AutoControlSetting))
        {
            OnPropertyChanged(nameof(AutoControl));
        }
        return;
    }

    /// <summary>
    /// Updates the CPU usage property when the CPU usage changes.
    /// </summary>
    private void CpuPower_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_powerInfo.CpuPower))
        {
            OnPropertyChanged(nameof(CpuPower));
        }
        return;
    }
    
    /// <summary>
    /// Updates the CPU usage property when the CPU usage changes.
    /// </summary>
    private void GpuPower_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_powerInfo.GpuPower))
        {
            OnPropertyChanged(nameof(GpuPower));
        }
        return;
    }

}
