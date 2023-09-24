using System.Diagnostics;
using EnergyPerformance.Helpers;
using Microsoft.Extensions.Hosting;

namespace EnergyPerformance.Services;

public class ProcessTrackerService : BackgroundService
{
    // Execute every 5 seconds
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMilliseconds(10000));
    private readonly ProcessTrackerInfo _processTrackerInfo;
    private readonly CpuInfo _cpuInfo;
    private readonly GpuInfo _gpuInfo;

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _periodicTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            await DoAsync();
        }
    }

    public ProcessTrackerService(CpuInfo cpuInfo, GpuInfo gpuInfo, ProcessTrackerInfo processTrackerInfo)
    {
        _cpuInfo = cpuInfo;
        _gpuInfo = gpuInfo;
        _processTrackerInfo = processTrackerInfo;

    }

    private async Task DoAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                foreach (var (process, counter) in _processTrackerInfo.ProcessCpuCounters)
                {
                    try
                    {
                        _cpuInfo.ProcessesCpuUsage[process.ProcessName] =
                            Math.Round(Math.Min(counter.NextValue(), 100.0), 2);
                    } catch (InvalidOperationException)
                    {
                        _processTrackerInfo.ProcessCpuCounters.Remove(process);
                    }
                }

                foreach (var (process, counter) in _processTrackerInfo.ProcessGpuCounters)
                {
                    try
                    {
                        _gpuInfo.ProcessesGpuUsage[process.ProcessName] =
                            Math.Round(Math.Min(counter.NextValue(), 100.0), 2);
                    }
                    catch (InvalidOperationException)
                    {
                        _processTrackerInfo.ProcessGpuCounters.Remove(process);
                    }
                }
            });
        } catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}