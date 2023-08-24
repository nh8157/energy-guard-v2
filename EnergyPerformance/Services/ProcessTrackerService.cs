using System.Diagnostics;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using EnergyPerformance.ViewModels;
using Microsoft.Extensions.Hosting;

namespace EnergyPerformance.Services;

public class ProcessTrackerService : BackgroundService
{
    // Execute every 5 seconds
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMilliseconds(10000));
    private readonly Dictionary<Process, PerformanceCounter> processCpuCounters = new();
    private readonly Dictionary<Process, PerformanceCounter> processGpuCounters = new();
    private readonly CpuInfo cpuInfo;
    private readonly GpuInfo gpuInfo;
    private readonly PersonaModel personaModel;
    

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _periodicTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            await DoAsync();
        }
    }
    
    public void AddProcess(Process process)
    {
        var cpuInstanceName = GetCpuInstanceNameForProcess(process);
        var gpuInstanceName = GetGpuInstanceNameForProcess(process);

        processCpuCounters.TryAdd(process, 
            new PerformanceCounter("Process", "% Processor Time", cpuInstanceName));

        if (gpuInstanceName == null)
        {
            return;
        }
        
        processGpuCounters.TryAdd(process, 
            new PerformanceCounter("GPU Engine", "Utilization Percentage", gpuInstanceName));
    }
    
    public void RemoveProcess(Process process)
    {
        processCpuCounters.Remove(process);
        processGpuCounters.Remove(process);
    }

    public ProcessTrackerService(CpuInfo cpuInfo, GpuInfo gpuInfo, PersonaModel personaModel)
    {
        this.cpuInfo = cpuInfo;
        this.gpuInfo = gpuInfo;
        this.personaModel = personaModel;

        var processes = this.personaModel.ReadPersonaAndRating();
        
        // Create a performance counter for each already running process in the persona list
        foreach (var (path, _) in processes)
        {
            var processName = Path.GetFileName(path);
            if (processName.Contains(".exe"))
                processName = processName.Remove(processName.Length - ".exe".Length);
            
            var process = Process.GetProcessesByName(processName).FirstOrDefault();

            App.GetService<DebugModel>().AddMessage($"Added tracker for {processName}");
            if (process == null)
            {
                App.GetService<DebugModel>().AddMessage("Process name is null");
                continue;
            }
            AddProcess(process);
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
                // App.GetService<DebugModel>().AddMessage($"Calculating per app usage {processCpuCounters.Keys.Count()}");

                foreach (var (process, counter) in processCpuCounters)
                {
                    cpuInfo.ProcessesCpuUsage[process.ProcessName] =
                        Math.Round(Math.Min(counter.NextValue(), 100.0), 2);
                    // App.GetService<DebugModel>().AddMessage($"Cpu usage of {process} is {cpuInfo.ProcessesCpuUsage[process.ProcessName]}");
                }

                foreach (var (process, counter) in processGpuCounters)
                {
                    gpuInfo.ProcessesGpuUsage[process.ProcessName] =
                        Math.Round(Math.Min(counter.NextValue(), 100.0), 2);
                    // App.GetService<DebugModel>().AddMessage($"Gpu usage of {process} is {gpuInfo.ProcessesGpuUsage[process.ProcessName]}");
                }
            });
        } catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}