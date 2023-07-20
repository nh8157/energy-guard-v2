using System.ComponentModel;

namespace EnergyPerformance.Helpers;

public class GpuInfo
{
    private double _gpuUsage;

    public event PropertyChangedEventHandler? CpuUsageChanged;

    public double GpuUsage
    {
        get => _gpuUsage;
        set
        {
            _gpuUsage = value;
            CpuUsageChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GpuUsage)));
        }
    }
    
    /// <summary>
    /// The current CPU usage of each process running on the system.
    /// </summary>
    public Dictionary<string, double> ProcessesGpuUsage { get; set; }
    
    public GpuInfo()
    {
        ProcessesGpuUsage = new Dictionary<string, double>();
        GpuUsage = 0;
    }
}