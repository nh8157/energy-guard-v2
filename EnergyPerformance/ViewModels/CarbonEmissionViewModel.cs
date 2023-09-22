using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using EnergyPerformance.Services;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Hosting;

namespace EnergyPerformance.ViewModels;

/// <summary>
/// View model responsible for the main application view.
/// </summary>
public partial class CarbonEmissionViewModel : ObservableRecipient
{

    [ObservableProperty]
    private double budgetUsedPercent;
    private double _carbonEmissions;
    private double _carbonEmissionsCpu;
    private double _carbonEmissionsGpu;

    private readonly EnergyUsageModel _model;
    private readonly ILocalSettingsService _settingsService;
    private readonly CpuInfo _cpuInfo;
    private readonly GpuInfo _gpuInfo;
    private readonly CarbonIntensityInfo _carbonIntensityInfo;

    private readonly PowerInfo _powerInfo;
    private readonly IAppNotificationService _notificationService;

    // must be a supported CPU and and auto control setting must be enabled in order to allow profile switching
    public bool AutoControl => _settingsService.AutoControlSetting && _cpuInfo.IsSupported;
    public double CpuUsage => _cpuInfo.CpuUsage;
    public double GpuUsage => _gpuInfo.GpuUsage;
    public double Power => _powerInfo.Power;
    public double CarbonEmissions => _carbonEmissions;
    public double CarbonEmissionsCpu => _carbonEmissionsCpu;
    public double CarbonEmissionsGpu => _carbonEmissionsGpu;

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
    public CarbonEmissionViewModel(PowerInfo powerInfo, CpuInfo cpu, GpuInfo gpu, 
        ILocalSettingsService settingsService, IAppNotificationService notificationService,
        EnergyUsageModel model, CarbonIntensityInfo carbonIntensity)
    {
        // set hardware info containers
        _powerInfo = powerInfo;
        _cpuInfo = cpu;
        _gpuInfo = gpu;
        _carbonIntensityInfo = carbonIntensity;

        _carbonEmissions = 0.0;
        _carbonEmissionsCpu = 0.0;
        _carbonEmissionsGpu = 0.0;

        // set services
        _settingsService = settingsService;
        _notificationService = notificationService;

        // set model
        _model = model;
        
        // attach event handlers
        _powerInfo.PowerUsageChanged += Power_PropertyChanged;
        _cpuInfo.CpuUsageChanged += CpuUsage_PropertyChanged;
        _gpuInfo.GpuUsageChanged += GpuUsage_PropertyChanged;
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
            _carbonEmissions = _powerInfo.Power * _carbonIntensityInfo.CarbonIntensity;
            OnPropertyChanged(nameof(CarbonEmissions));
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
    private void CpuUsage_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_cpuInfo.CpuUsage))
        {
            _carbonEmissionsCpu = Math.Round(_cpuInfo.CpuPower * _carbonIntensityInfo.CarbonIntensity, 2);
            OnPropertyChanged(nameof(CarbonEmissionsCpu));
        }
        return;
    }
    
    /// <summary>
    /// Updates the CPU usage property when the CPU usage changes.
    /// </summary>
    private void GpuUsage_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_gpuInfo.GpuUsage))
        {
            _carbonEmissionsGpu = Math.Round(_gpuInfo.GpuUsage * _carbonIntensityInfo.CarbonIntensity, 2);
            OnPropertyChanged(nameof(CarbonEmissionsGpu));
        }
        return;
    }

}
