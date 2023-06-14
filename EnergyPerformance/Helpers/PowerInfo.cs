using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyPerformance.Helpers;

/// <summary>
/// Container class to hold the current power usage of the system.
/// </summary>
public class PowerInfo
{
    private double _power;
    public event PropertyChangedEventHandler? PowerUsageChanged;

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

    public PowerInfo()
    {
        Power = 0;
    }

}
