using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyPerformance.Elevated.HardwareMonitor;
using EnergyPerformance.Elevated.MessageHandlers;
using LibreHardwareMonitor.Hardware;

namespace EnergyPerformance.Elevated;

public class MonitorHandler: MessageHandler
{
    public Computer computer;
    private List<ISensor> gpuPowerSensors;
    private List<ISensor> cpuPowerSensors;
    private ISensor gpuUsageSensor;
    
    public MonitorHandler()
    {
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
        
        gpuPowerSensors = new List<ISensor>();
        cpuPowerSensors = new List<ISensor>();
        
        computer.Open();
        computer.Accept(new UpdateVisitor());
        foreach (IHardware hardware in computer.Hardware)
        {
            hardware.Update();
            foreach (ISensor sensor in hardware.Sensors)
            {
                // read CPU sensors which report power values
                if (sensor.Name.Contains("Package") && sensor.SensorType.Equals(SensorType.Power) && hardware.HardwareType.Equals(HardwareType.Cpu))
                {
                    // Sensor objects which report power values are added to a list so that they can be referenced later.
                    cpuPowerSensors.Add(sensor);
                }

                // read GPU sensors which report power values
                if (sensor.Name.Contains("GPU Package") && sensor.SensorType.Equals(SensorType.Power))
                {
                    // Sensor objects which report power values are added to a list so that they can be referenced later.
                    gpuPowerSensors.Add(sensor);
                }
                
                // read GPU sensors which report usage values
                if (sensor.Name.Contains("GPU Core") && sensor.SensorType.Equals(SensorType.Load))
                {
                    gpuUsageSensor = sensor;
                }
            }
        }
    }
    
    private double GetCpuPower()
    {
        double cpuPower = 0;
        
        foreach (IHardware hardware in computer.Hardware)
        {
            hardware.Update();
        }
        
        foreach (ISensor sensor in cpuPowerSensors)
        {
            cpuPower += sensor.Value ?? 0;
        }
        return cpuPower;
    }
    
    private double GetGpuPower()
    {
        double gpuPower = 0;
        
        foreach (IHardware hardware in computer.Hardware)
        {
            hardware.Update();
        }
        
        foreach (ISensor sensor in gpuPowerSensors)
        {
            gpuPower += sensor.Value ?? 0;
        }
        return gpuPower;
    }
    
    private double GetGpuUsage()
    {
        foreach (IHardware hardware in computer.Hardware)
        {
            hardware.Update();
        }
        
        return gpuUsageSensor.Value ?? 0;;
    }

    public Task<string?> HandleMessage(string message)
    {
        // The message is a string containing the name of the function to be called.
        double? response = message switch
        {
            "GetCpuPower" => GetCpuPower(),
            "GetGpuPower" => GetGpuPower(),
            "GetGpuUsage" => GetGpuUsage(),
            _ => null
        };

        return Task.FromResult(response is not null ? response.ToString() : null);
    }
}