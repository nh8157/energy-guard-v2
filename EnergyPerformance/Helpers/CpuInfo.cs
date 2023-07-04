using System.ComponentModel;
using System.Diagnostics;
//using EnergyPerformance.Temporary;

namespace EnergyPerformance.Helpers;

/// <summary>
/// Container class to hold the current CPU usage of the system and provide access to the Controller class in C++/CLI
/// to perform hybrid core shift operations.
/// </summary>
public class CpuInfo
{
    public readonly Controller CpuController; // C++/CLI Controller class provided here to make CPUTrackerService more testable.
    public bool IsSupported { get; set; }
    private double _cpuUsage;
    public event PropertyChangedEventHandler? CpuUsageChanged;

    /// <summary>
    /// The current CPU usage of the system.
    /// Invokes all functions subscribed to the CpuUsageChanged event when the CPU usage value changes.
    /// Used for updating the View when the CPU usage changes in the CPUTrackerService.
    /// </summary>
    public double CpuUsage
    {
        get => _cpuUsage;
        set
        {
            _cpuUsage = value;
            CpuUsageChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CpuUsage)));
        }
    }

    public CpuInfo()
    {
        CpuController = new Controller();
        IsSupported = CheckProcessorIsSupported();
        CpuUsage = 0;
    }

    /// <summary>
    /// Checks if the processor is supported by the application
    /// </summary>
    /// <returns>True if the processor is supported, false otherwise</returns>
    /// <remarks> The processor checks for the number of physical P-Cores here from the CLI controller
    /// as acquiring hyper-threaded cores allows processors below 12th gen to be supported.
    /// </remarks>
    private bool CheckProcessorIsSupported()
    {
        var pCores = CpuController.PerformanceCoreCount(); 
        var eCores = CpuController.EfficiencyCoreCount();
        if (pCores < 2 || eCores < 4 || pCores % 2 != 0 || eCores % 2 != 0)
        {
            return false;
        }
        Debug.WriteLine("Processor Supported");
        return true;
    }


}
