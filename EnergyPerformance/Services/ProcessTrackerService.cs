using System.Diagnostics;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using Microsoft.Extensions.Hosting;

namespace EnergyPerformance.Services;

public class ProcessTrackerService : BackgroundService
{
    // Execute every 5 seconds
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMilliseconds(10000));
    private readonly Dictionary<Process, PerformanceCounter> processCpuCounters = new();
    private readonly Dictionary<Process, PerformanceCounter> processGpuCounters = new();
    private readonly string[] processBlacklist =
    {
        "Idle", "System", "svchost", "Registry", "smss", "csrss", "wininit", "services", "lsass", "winlogon",
        "fontdrvhost", "dwm", "taskhostw", "sihost", "explorer", "SearchApp", "SearchIndexer", "RuntimeBroker",
    };

    private readonly CpuInfo cpuInfo;
    private readonly GpuInfo gpuInfo;
    
    private readonly DebugModel _debug = App.GetService<DebugModel>();

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _debug.AddMessage("GPU Tracker Service Started");
        while (await _periodicTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            await DoAsync();
        }
    }

    public ProcessTrackerService(CpuInfo cpuInfo, GpuInfo gpuInfo)
    {
        this.cpuInfo = cpuInfo;
        this.gpuInfo = gpuInfo;
        
        // Create a performance counter for all user processes
        var processes = Process.GetProcesses();
        processes = processes.Where(process => !processBlacklist.Contains(process.ProcessName)).ToArray();

        foreach (var process in processes)
        {
            var cpuInstanceName = GetCpuInstanceNameForProcess(process);
            var gpuInstanceName = GetGpuInstanceNameForProcess(process);

            processCpuCounters.Add(process, 
                new PerformanceCounter("Process", "% Processor Time", cpuInstanceName));

            if (gpuInstanceName == null)
            {
                continue;
            }
            
            processGpuCounters.Add(process, 
                new PerformanceCounter("GPU Engine", "Utilization Percentage", gpuInstanceName));
        }
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
    
    private async Task DoAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                foreach (var (process, counter) in processCpuCounters)
                {
                    cpuInfo.ProcessesCpuUsage[process.ProcessName] =
                        Math.Round(Math.Min(counter.NextValue(), 100.0), 2);
                }

                foreach (var (process, counter) in processGpuCounters)
                {
                    gpuInfo.ProcessesGpuUsage[process.ProcessName] =
                        Math.Round(Math.Min(counter.NextValue(), 100.0), 2);
                }
            });
        } catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}