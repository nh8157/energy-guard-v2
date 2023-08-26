using System.Diagnostics;
using EnergyPerformance.Helpers;

namespace EnergyPerformance.Helpers;
public class ProcessTrackerInfo
{
    private Dictionary<Process, PerformanceCounter> _processCpuCounters;
    private Dictionary<Process, PerformanceCounter> _processGpuCounters;

    public Dictionary<Process, PerformanceCounter> ProcessCpuCounters => _processCpuCounters;
    public Dictionary<Process, PerformanceCounter> ProcessGpuCounters => _processGpuCounters;

    public ProcessTrackerInfo()
    {
        _processCpuCounters = new();
        _processGpuCounters = new();
    }
    public void AddProcess(Process process)
    {
        var cpuInstanceName = GetCpuInstanceNameForProcess(process);
        var gpuInstanceName = GetGpuInstanceNameForProcess(process);

        _processCpuCounters.TryAdd(process, 
            new PerformanceCounter("Process", "% Processor Time", cpuInstanceName));

        if (gpuInstanceName == null)
        {
            return;
        }
        
        _processGpuCounters.TryAdd(process, 
            new PerformanceCounter("GPU Engine", "Utilization Percentage", gpuInstanceName));
    }
    public void RemoveProcess(string process)
    {
        _processCpuCounters = _processCpuCounters.Where(kv => kv.Key.ProcessName != process).ToDictionary(kv => kv.Key, kv => kv.Value);
        _processGpuCounters = _processGpuCounters.Where(kv => kv.Key.ProcessName != process).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private string GetCpuInstanceNameForProcess(Process process)
    {
        var instanceName = process.ProcessName;
        var processesByName = Process.GetProcessesByName(instanceName);

        if (processesByName.Length <= 1) return instanceName;

        var processCategory = new PerformanceCounterCategory("Process");

        foreach (var currentInstance in processCategory.GetInstanceNames().Where(currentInstance => currentInstance.StartsWith(instanceName)))
        {
            using var processIdCounter = new PerformanceCounter("Process", "ID Process", currentInstance);
            
            if ((int)processIdCounter.RawValue != process.Id)
            {
                continue;
            }
            
            instanceName = currentInstance;
            break;
        }

        return instanceName;
    }

    private string? GetGpuInstanceNameForProcess(Process process)
    {
        var instanceName = $"pid_{process.Id}";
        
        var processCategory = new PerformanceCounterCategory("GPU Engine");
        var instanceNames = processCategory.GetInstanceNames().Where(currentInstance =>
            currentInstance.StartsWith(instanceName) && currentInstance.EndsWith("engtype_3D"));

        return instanceNames.FirstOrDefault();
    }
}
