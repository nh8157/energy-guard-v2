using System.ComponentModel;
using Windows.System;


namespace EnergyPerformance.Helpers;

/// <summary>
/// Container class to hold the current power usage of the system.
/// </summary>
public class PowerInfo
{
    private double _power;
    private double _gpuPower;
    private double _cpuPower;
    public event PropertyChangedEventHandler? PowerUsageChanged;
    public event PropertyChangedEventHandler? GpuPowerUsageChanged;
    public event PropertyChangedEventHandler? CpuPowerUsageChanged;

    /// <summary>
    /// The current power usage of the system.
    /// Invokes all functions subscribed to the PowerUsageChanged event when the Power usage value changes.
    /// Used for updating the View when the power usage changes in the PowerMonitorService.
    /// </summary>
    public virtual double Power
    {
        get => _power;
        set
        {
            _power = value;
            PowerUsageChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Power)));
        }
    }
    
    public virtual double GpuPower
    {
        get => _gpuPower;
        set
        {
            _gpuPower = value;
            GpuPowerUsageChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GpuPower)));
        }
    }

    public virtual double CpuPower
    {
        get => _cpuPower;
        set
        {
            _cpuPower = value;
            CpuPowerUsageChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CpuPower)));
        }
    }

    public PowerInfo()
    {
        Power = 0;
        GpuPower = 0;
        CpuPower = 0;
    }

}
