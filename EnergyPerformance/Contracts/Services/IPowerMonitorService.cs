using System.ComponentModel;
using EnergyPerformance.Helpers;
using Microsoft.Extensions.Hosting;
using static EnergyPerformance.ViewModels.MainViewModel;

namespace EnergyPerformance.Contracts.Services;

public interface IPowerMonitorService
{
    public double Power
    {
        get;
    }

    public void DetectRequiredSensors();
}
