using System;
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
    private readonly Computer computer;
    private readonly EnergyUsageModel _model;
    private readonly PowerInfo _powerInfo;
    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private const string _defaultApplicationDataFolder = "EnergyPerformance/ApplicationData";

    public double Power
    {
        get => _powerInfo.Power;
        private set => _powerInfo.Power = value;
    }


    /// <summary>
    /// Visitor class used to update the hardware components of the system.
    /// </summary>
    /// <see href="https://github.com/LibreHardwareMonitor/LibreHardwareMonitor">Reference to LibreHardwareMonitor</see>
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor)
        {
        }
        public void VisitParameter(IParameter parameter)
        {
        }
    }

    /// <summary>
    /// Constructor for the PowerMonitorService class.
    /// </summary>
    /// <param name="model"><see cref="EnergyUsageModel"/> to contain data for the accumulated power usage of the system</param>
    /// <param name="powerInfo"><see cref="PowerInfo"/> to contain live power data for the system, for the view.</param>
    public PowerMonitorService(EnergyUsageModel model, PowerInfo powerInfo)
    {
        _model = model;
        _powerInfo = powerInfo;
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
                    Debug.WriteLine("Hardware: {0}", hardware.Name);
                    Debug.WriteLine("\t\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);

                    // Sensor objects which report power values are added to a list so that they can be referenced later.
                    sensors.Add(sensor);
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
        // Call the update hardware method to update sensor data
        foreach (IHardware hardware in computer.Hardware)
        {
            hardware.Update();
        }
        // use a local variable to track the accumulated power values from the sensors,
        // using the Power property instead will notify listeners each time the value is updated.
        double power = 0;
        // Get the latest value from all available sensors which read power values
        foreach (ISensor sensor in sensors)
        {
            power += sensor.Value ?? 0; // return 0 if null;
        }

        Power = power; // update the front end power value only with the value read from sensors
        var currentDateTime = DateTimeOffset.Now;
        // methods to update the daily and hourly power usage

        // TODO: record carbon emissions
        UpdateDailyUsage(currentDateTime, Power);
        UpdateHourlyUsage(currentDateTime, Power);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Method to update the daily power usage in the model.
    /// </summary>
    /// <param name="currentDateTime">DateTimeOffset object</param>
    /// <param name="power">Contains the current power value used to update the model</param>
    /// <returns></returns>
    private void UpdateDailyUsage(DateTimeOffset currentDateTime, double power)
    {
        if (power < 0)
        {
            power = 0;
        }
        // accumulate watts if same day
        if (currentDateTime.DateTime.Date == _model.CurrentDay.DateTime.Date)
        {
            _model.AccumulatedWatts += power;
        }
        // set date to the new day, and reset acc. watts the power just measured
        else
        {
            _model.CurrentDay = currentDateTime;
            _model.AccumulatedWatts = power;
        }
    }

    /// <summary>
    /// Method to update the hourly power usage in the model.
    /// </summary>
    private void UpdateHourlyUsage(DateTimeOffset currentDateTime, double power)
    {
        if (power < 0)
        {
            power = 0;
        }
        // accumulate watts if the same hour
        if (currentDateTime.DateTime.Hour == _model.CurrentHour.DateTime.Hour)
        {
            _model.AccumulatedWattsHourly += power;
        }
        // set time to the current time, and reset acc. watts to the power just measured
        else
        {
            _model.CurrentHour = currentDateTime;
            _model.AccumulatedWattsHourly = power;
        }
    }
}
