using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;

namespace EnergyPerformance.Services;

/// <summary>
/// This class is responsible for tracking the current CPU usage and performing automatic or manual switching between energy profiles.
/// It is a background service that runs on a separate thread and is executed periodically, on a per-second basis.
/// </summary>
public class CpuTrackerService : BackgroundService
{
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMilliseconds(1000));
    private readonly PerformanceCounter totalPerformanceCounter;
    private readonly int _totalCores;
    private const int CpuUsageDoublePrecision = 2;
    private readonly CpuInfo _cpuInfo;

    public const int Duration = 30;

    public bool SupportedCpu => _cpuInfo.IsSupported;


    private double CpuUsage
    {
        set => _cpuInfo.CpuUsage = value;
    }

    public CpuTrackerService(CpuInfo cpuInfo)
    {
        _cpuInfo = cpuInfo;
        totalPerformanceCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
        _totalCores = _cpuInfo.CpuController.TotalCoreCount();
    }

    protected async override Task ExecuteAsync(CancellationToken token)
    {
        while (await _periodicTimer.WaitForNextTickAsync(token) && !token.IsCancellationRequested)
        {
            await DoAsync();
        }
    }

    /// <summary>
    ///  Main function which is called every 1 second based on PeriodicTimer.
    ///  Performs CPU usage tracking and switches the processor to the appropriate mode.
    /// </summary>
    /// <returns></returns>
    private async Task DoAsync()
    {
        CpuUsage = Math.Round(Math.Min(totalPerformanceCounter.NextValue(), 100.0), CpuUsageDoublePrecision);

        await Task.CompletedTask;
    }
}
