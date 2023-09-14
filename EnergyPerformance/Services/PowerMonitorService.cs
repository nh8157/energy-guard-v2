﻿using System;
using System.IO;
using System.Diagnostics;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using EnergyPerformance.ViewModels;
using EnergyPerformance.Views;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Hosting;


namespace EnergyPerformance.Services;

/// <summary>
/// Hosted Service that monitors the power usage of the system, with periodic reporting every second.
/// </summary>
public class PowerMonitorService : BackgroundService, IPowerMonitorService
{
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMilliseconds(1000));
    public List<ISensor> sensors;
    public List<ISensor> cpuSensors;
    public List<ISensor> gpuSensors;
    private readonly Computer computer;
    private readonly EnergyUsageModel _model;

    private readonly PowerInfo _powerInfo;
    private readonly CpuInfo _cpuInfo;
    private readonly GpuInfo _gpuInfo;
    
    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private const string _defaultApplicationDataFolder = "EnergyPerformance/ApplicationData";

    public double Power
    {
        get => _powerInfo.Power;
        private set => _powerInfo.Power = value;
    }

    // We want to also track the power usage of the CPU and GPU separately, so we can display them in the View.
    public double CpuPower { get; private set; }
    public double GpuPower { get; private set; }

    
    /// <summary>
    /// Constructor for the PowerMonitorService class.
    /// </summary>
    /// <param name="model"><see cref="EnergyUsageModel"/> to contain data for the accumulated power usage of the system</param>
    /// <param name="powerInfo"><see cref="PowerInfo"/> to contain live power data for the system, for the view.</param>
    /// <param name="cpuInfo"><see cref="CpuInfo"/> to contain live CPU data for the system, for the view.</param>
    /// <param name="gpuInfo"><see cref="GpuInfo"/> to contain live GPU data for the system, for the view.</param>
    public PowerMonitorService(EnergyUsageModel model, PowerInfo powerInfo, CpuInfo cpuInfo, GpuInfo gpuInfo)
    {
        _model = model;
        _powerInfo = powerInfo;
        _cpuInfo = cpuInfo;
        _gpuInfo = gpuInfo;
        // configure computer object to monitor hardware components
        computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsNetworkEnabled = true,
            IsStorageEnabled = true
        };
        sensors = new List<ISensor>();
        cpuSensors = new List<ISensor>();
        gpuSensors = new List<ISensor>();
    }

    /// <summary>
    /// Method to detect sensors which report power values in the device.
    /// </summary>
    public void DetectRequiredSensors()
    {
        computer.Open();
        computer.Accept(new UpdateVisitor());
        foreach (IHardware hardware in computer.Hardware)
        {
            hardware.Update();
            foreach (ISensor sensor in hardware.Sensors)
            {
                // read hardware sensors which report power values
                if ((sensor.Name.Contains("Package") || sensor.Name.Contains("Power")) && sensor.SensorType.Equals(SensorType.Power))
                {
                    // Sensor objects which report power values are added to a list so that they can be referenced later.
                    sensors.Add(sensor);
                }

                // read CPU sensors which report power values
                if (sensor.Name.Contains("Package") && sensor.SensorType.Equals(SensorType.Power) && hardware.HardwareType.Equals(HardwareType.Cpu))
                {
                    // Sensor objects which report power values are added to a list so that they can be referenced later.
                    cpuSensors.Add(sensor);
                }

                // read GPU sensors which report power values
                if (sensor.Name.Contains("GPU Package") && sensor.SensorType.Equals(SensorType.Power))
                {
                    // Sensor objects which report power values are added to a list so that they can be referenced later.
                    gpuSensors.Add(sensor);
                }
            }
        }
    }

    /// <summary>
    /// Method called when the Hosted Service is started, periodically calls the DoAsync method to 
    /// perform power monitoring operations.
    /// </summary>
    /// <param name="token">Cancellation token to stop the service.</param>
    /// <returns></returns>
    protected async override Task ExecuteAsync(CancellationToken token)
    {
        DetectRequiredSensors();
        while (await _periodicTimer.WaitForNextTickAsync(token) && !token.IsCancellationRequested)
        {
            await DoAsync();
        }
        computer.Close();
    }

    /// <summary>
    /// Method called every second to update the power usage of the system.
    /// </summary>
    private async Task DoAsync()
    {
        // Run the hardware update and power computation in a separate thread to improve performance
        await Task.Run(() =>
        {
            foreach (IHardware hardware in computer.Hardware)
            {
                hardware.Update();
            }
            // GPU power usage
            double gpuPower = 0;
            foreach (ISensor sensor in gpuSensors)
            {
                gpuPower += sensor.Value ?? 0;
            }

            GpuPower = gpuPower;

            // CPU power usage
            double cpuPower = 0;
            foreach (ISensor sensor in cpuSensors)
            {
                cpuPower += sensor.Value ?? 0;
            }
            CpuPower = cpuPower;

        });

        Power = CpuPower + GpuPower;

        var currentDateTime = DateTimeOffset.Now;

        UpdateDailyUsage(currentDateTime);
        UpdateHourlyUsage(currentDateTime);
    }

    /// <summary>
    /// Method to update the daily power usage in the model.
    /// </summary>
    /// <param name="currentDateTime">DateTimeOffset object</param>
    /// <param name="power">Contains the current power value used to update the model</param>
    /// <returns></returns>
    private void UpdateDailyUsage(DateTimeOffset currentDateTime)
    {
        if (Power < 0)
        {
            Power = 0;
            foreach (var (process, _) in _model.AccumulatedWattsPerApp)
            {
                _model.AccumulatedWattsPerApp[process] = 0;
            }
        }
        // accumulate watts if same day
        if (currentDateTime.DateTime.Date == _model.CurrentDay.DateTime.Date)
        {
            _model.AccumulatedWatts += Power;

            // calculate an app's CPU power
            foreach (var (process, _) in _cpuInfo.ProcessesCpuUsage)
            {
                var cpuUsage = _cpuInfo.ProcessesCpuUsage.GetValueOrDefault(process);
                var accWatts = _model.AccumulatedWattsPerApp.GetValueOrDefault(process);
                _model.AccumulatedWattsPerApp[process] = accWatts + cpuUsage/100 * CpuPower;
            }

            // calculate an app's GPU power
            foreach (var (process, _) in _gpuInfo.ProcessesGpuUsage)
            {
                var gpuUsage = _gpuInfo.ProcessesGpuUsage.GetValueOrDefault(process);
                var accWatts = _model.AccumulatedWattsPerApp.GetValueOrDefault(process);
                _model.AccumulatedWattsPerApp[process] = accWatts + gpuUsage/100 * GpuPower;
            }
        }
        // set date to the new day, and reset acc. watts the power just measured
        else
        {
            _model.CurrentDay = currentDateTime;
            _model.AccumulatedWatts = Power;

            // calculate an app's CPU power
            foreach (var (process, _) in _cpuInfo.ProcessesCpuUsage)
            {
                var cpuUsage = _cpuInfo.ProcessesCpuUsage.GetValueOrDefault(process);
                var accWatts = _model.AccumulatedWattsPerApp.GetValueOrDefault(process);
                _model.AccumulatedWattsPerApp[process] = accWatts + cpuUsage/100 * CpuPower;
            }

            // calculate an app's GPU power
            foreach (var (process, _) in _gpuInfo.ProcessesGpuUsage)
            {
                var gpuUsage = _gpuInfo.ProcessesGpuUsage.GetValueOrDefault(process);
                var accWatts = _model.AccumulatedWattsPerApp.GetValueOrDefault(process);
                _model.AccumulatedWattsPerApp[process] = accWatts + gpuUsage/100 * GpuPower;
            }
        }
    }

    /// <summary>
    /// Method to update the hourly power usage in the model.
    /// </summary>
    private void UpdateHourlyUsage(DateTimeOffset currentDateTime)
    {
        if (Power < 0)
        {
            Power = 0;
        }
        // accumulate watts if the same hour
        if (currentDateTime.DateTime.Hour == _model.CurrentHour.DateTime.Hour)
        {
            _model.AccumulatedWattsHourly += Power;
        }
        // set time to the current time, and reset acc. watts to the power just measured
        else
        {
            _model.CurrentHour = currentDateTime;
            _model.AccumulatedWattsHourly = Power;
        }
    }
}
