using EnergyPerformance.Helpers;
using Microsoft.Extensions.Hosting;

namespace EnergyPerformance.Services;

// User LibreHardwareMonitor to track the total GPU percentage usage of the system.
public class GpuTrackerService : BackgroundService
{
    // Execute every second
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMilliseconds(1000));
    private readonly GpuInfo gpuInfo;
    private readonly MonitorController monitorController;
    private double lastUsage = 0;

    public double GpuUsage
    {
        get => gpuInfo.GpuUsage;
        private set => gpuInfo.GpuUsage = value;
    }

    public GpuTrackerService(GpuInfo gpuInfo, MonitorController monitorController)
    {
        this.gpuInfo = gpuInfo;
        this.monitorController = monitorController;
    }
    
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _periodicTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            await DoAsync();
        }
    }

    private async Task DoAsync()
    {
        var usage = monitorController.GetGpuUsage();
        GpuUsage = usage == 0 ? lastUsage : usage;
        lastUsage = GpuUsage;

        await Task.CompletedTask;
    }
}