using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;

namespace EnergyPerformance.Services;

/// <summary>
/// This class is responsible for tracking the current CPU usage and performing automatic or manual switching between energy profiles.
/// It is a background service that runs on a separate thread and is executed periodically, on a per-second basis.
/// </summary>
public class CpuTrackerService : BackgroundService, ICpuTrackerService
{
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMilliseconds(1000));
    private readonly PerformanceCounter totalPerformanceCounter;
    private readonly ILocalSettingsService _settingsService;
    private readonly IAppNotificationService _notificationService;
    private int runningAverageCounter;
    private double runningAverage;
    private int eCoresActive;
    private int pCoresActive;
    private int currentMode;
    private readonly int _totalCores;
    private int newMode;
    private const int CpuUsageDoublePrecision = 2;
    private const double IncrementModeThreshold = 0.5;
    private const double DecrementModeThreshold = 0.25;
    private const double CasualModeECorePercentage = 0.5;
    private const int DefaultCasualModeThreshold = 30;
    private const int DefaultWorkModeThreshold = 70;
    private readonly CpuInfo _cpuInfo;

    public const int Duration = 30;

    public bool SupportedCpu => _cpuInfo.IsSupported;

    public int CurrentMode => currentMode;


    private double CpuUsage
    {
        get => _cpuInfo.CpuUsage;
        set => _cpuInfo.CpuUsage = value;
    }

    public CpuTrackerService(ILocalSettingsService settingsService, IAppNotificationService notificationService, CpuInfo cpuInfo)
    {
        _settingsService = settingsService;
        _notificationService = notificationService;
        runningAverage = 0; runningAverageCounter = 0;
        eCoresActive = 0; pCoresActive = 0;
        _cpuInfo = cpuInfo;
        totalPerformanceCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
        _totalCores = _cpuInfo.CpuController.TotalCoreCount();
        currentMode = 2;

    }

    protected async override Task ExecuteAsync(CancellationToken token)
    {
        while (await _periodicTimer.WaitForNextTickAsync(token) && !token.IsCancellationRequested)
        {
            await DoAsync();
        }
    }

    /// <summary>
    /// Switches the processor to "casual" mode using the CLI controller.
    /// This mode uses 50% of the available eCores and 0% of the pCores.
    /// </summary>
    private void CasualMode()
    {
        currentMode = 0;
        var cores = CasualModeECorePercentage * _cpuInfo.CpuController.EfficiencyCoreCount();
        // Round to the nearest even integer
        eCoresActive = (int)Math.Round(cores / 2, MidpointRounding.AwayFromZero) * 2;
        pCoresActive = 0;
        // Can we control P-core E-core settings at the granularity of processes
        _cpuInfo.CpuController.MoveAllAppsToHybridCores(eCoresActive, pCoresActive);
    }


    /// <summary>
    /// Switches the processor to "work" mode using the CLI controller.
    /// This mode uses 100% of the available eCores and 0% of the pCores.
    /// </summary>
    private void WorkMode()
    {
        currentMode = 1;
        eCoresActive = _cpuInfo.CpuController.EfficiencyCoreCount();
        pCoresActive = 0;

        _cpuInfo.CpuController.MoveAllAppsToHybridCores(eCoresActive, pCoresActive);
    }


    /// <summary>
    /// Switches the processor to "performance" mode using the CLI controller.
    /// This mode resets the processor to its default state, using all available cores.
    /// </summary>
    private void PerformanceMode()
    {
        currentMode = 2;
        eCoresActive = 0;
        pCoresActive = 0;
        _cpuInfo.CpuController.ResetToDefaultCores();
    }

    /// <summary>
    /// Helper function which bounds the argument value between the given min and max values.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns>Integer between min and max</returns>
    private int BoundedValue(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }
        else if (value > max)
        {
            return max;
        }
        return value;
    }


    /// <summary>
    /// Switches the processor to the appropriate mode based on the argument mode setting.
    /// </summary>
    /// <param name="mode">The input mode, either "casual", "work", "performance" or "auto".</param>
    public void ModeSwitch(string mode)
    {
        switch (mode)
        {
            case "Casual":
                CasualMode();
                break;
            case "Work":
                WorkMode();
                break;
            case "Performance":
                PerformanceMode();
                break;
        }
    }


    /// <summary>
    /// Automatically decides the current mode and shifts processor load based on the current CPU usage.
    /// Requires the selected mode to be "auto" and the processor to be supported.
    /// Switches to a specific mode if the CPU usage is within a certain threshold at startup.
    /// </summary>
    /// <param name="currentCpuUsage"></param>
    public async Task AutomaticModeControl(int activeCores, int totalCores, double currentCpuUsage)
    {
        var previousMode = currentMode;
        newMode = DetermineCurrentMode(activeCores, totalCores, currentMode, currentCpuUsage);
        if ((currentCpuUsage < DefaultCasualModeThreshold && activeCores == 0) || newMode == 0)
        {
            CasualMode();
        }
        else if ((currentCpuUsage < DefaultWorkModeThreshold && activeCores == 0) || newMode == 1)
        {
            WorkMode();
        }
        else
        {
            PerformanceMode();
        }

        if (previousMode != currentMode)
        {
            await NotifyModeChange(currentMode);
        }

    }

    private async Task NotifyModeChange(int mode)
    {
        string notificationName;
        if (mode == 0)
        {
            notificationName = "CasualModeNotification";
        } else if (mode == 1)
        {
            notificationName = "WorkModeNotification";
        } else
        {
            notificationName = "PerformanceModeNotification";
        }
        await _notificationService.ShowAsync(notificationName);
    }


    /// <summary>
    /// Determines the current mode based on the CPU usage and the number of active cores for load balancing.
    /// Calculates a weighted threshold to compare the CPU usage with, based on the number of available cores
    /// and the number of cores currently in use through load balancing operations.
    /// </summary>
    /// <param name="activeCores"></param>
    /// <param name="mode"></param>
    /// <param name="currentCpuUsage"></param>
    /// <returns></returns>
    private int DetermineCurrentMode(int activeCores, int totalCores, int mode, double currentCpuUsage)
    {
        if (activeCores <= 0)
        {
            return mode;
        }
        var newMode = mode;
        var threshold = (double)activeCores / (double)totalCores;
        threshold *= 100;
        // If CPU usage is above the percentage of threshold times the incrementmodethreshold
        // then increase the mode by one
        if (currentCpuUsage > threshold * IncrementModeThreshold)
        {
            newMode += 1;
            if (newMode > currentMode + 1)
            {
                newMode = currentMode + 1;
            }
        }
        // If CPU usage is below the percentage of threshold times the decrementmodethreshold
        // then decrease the mode by one
        else if (currentCpuUsage < DecrementModeThreshold * threshold)
        {
            newMode -= 1;
            if (newMode < currentMode - 1)
            {
                newMode = currentMode - 1;
            }
        }

        // make sure that the current mode is within the 3 defined modes
        if (mode == 0)
        {
            newMode = BoundedValue(newMode, 0, 1); 
        }
        else if (mode == 2)
        {
            newMode = BoundedValue(newMode, 1, 2);
        }
        else
        {
            newMode = BoundedValue(newMode, 0, 2);
        }

        return newMode;
    }

    /// <summary>
    ///  Main function which is called every 1 second based on PeriodicTimer.
    ///  Performs CPU usage tracking and switches the processor to the appropriate mode.
    /// </summary>
    /// <returns></returns>
    private async Task DoAsync()
    {
        CpuUsage = Math.Round(Math.Min(totalPerformanceCounter.NextValue(), 100.0), CpuUsageDoublePrecision);
        
        if (!SupportedCpu || !_settingsService.AutoControlSetting)
        {
            return;
        }


        /*
        if (!_settingsService.SelectedMode.Equals("Auto"))
        {
            ModeSwitch(_settingsService.SelectedMode);
            return;
        }
        var activeCores = eCoresActive + pCoresActive;

        // calculate a new running average of CPU usage every 30s
        // and call AutomaticModeControl with the newly calculated average at the end
        // of each 30s interval.
        if (runningAverageCounter != Duration)
        {
            runningAverageCounter++;
            runningAverage += CpuUsage;
        } else
        {
            runningAverageCounter = 0;
            runningAverage /= Duration;
            await AutomaticModeControl(activeCores, _totalCores, runningAverage);
            runningAverage = CpuUsage;
        }
        */
        
        // Where does the switching occur?
        // What is this line for?
        await Task.CompletedTask;
    }
}
