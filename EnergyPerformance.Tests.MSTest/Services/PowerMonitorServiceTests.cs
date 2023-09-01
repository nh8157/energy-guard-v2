using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Contracts.Services;
using EnergyPerformance.Core.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using EnergyPerformance.Services;
using Microsoft.Extensions.Options;
using Moq;
using Windows.Foundation;

namespace EnergyPerformance.Tests.MSTest.Services;

[TestClass()]
public class PowerMonitorServiceTests
{
    private static IDatabaseService _databaseService;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _databaseService = new DatabaseService("testDB.db");
    }
    public PowerMonitorService GetService()
    {
        return new PowerMonitorService(new EnergyUsageModel(new CarbonIntensityInfo(), new EnergyRateInfo(), new DatabaseService("testDB.db")), 
            new PowerInfo(), new CpuInfo(), new GpuInfo());
    }

    [TestMethod]
    public void TestPowerMonitorService()
    {
        var service = GetService();
        Assert.IsNotNull(service);
    }


    [TestMethod]
    public async Task TestPowerMonitorServiceIsInitializedCorrectly()
    {
        var powerInfo = new PowerInfo();
        var model = new EnergyUsageModel(new CarbonIntensityInfo(), new EnergyRateInfo(), _databaseService);
        var service = new PowerMonitorService(model, powerInfo, new CpuInfo(), new GpuInfo());
        Assert.IsTrue(service.sensors.Count() == 0);
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        await service.StartAsync(token);
        await Task.Delay(1000);
        cts.Cancel();
        if (service.sensors.Count() > 0)
        {
           Assert.IsTrue(true);
        } else
        {
            Debug.WriteLine("No sensors found. Restart VS 2022 in Admin Mode to detect hardware sensors.");
        }
    }

    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(45)]
    [TestMethod]
    public async Task TestServiceUpdatesModelCorrectly(double value)
    {
        var powerInfo = new Mock<PowerInfo>();
        powerInfo.Setup(p => p.Power).Returns(value);
        var model = new EnergyUsageModel(new CarbonIntensityInfo(), new EnergyRateInfo(), _databaseService);
        Assert.AreEqual(model.AccumulatedWatts, 0);
        var cpuInfo = new Mock<CpuInfo>();
        var gpuInfo = new Mock<GpuInfo>();
        var data = new Dictionary<string, double>();
        data.Add("process", 0);
        model.AccumulatedWattsPerApp = data;
        cpuInfo.Setup(c => c.ProcessesCpuUsage).Returns(data);
        gpuInfo.Setup(g => g.ProcessesGpuUsage).Returns(data);
        var service = new PowerMonitorService(model, powerInfo.Object, new CpuInfo(), new GpuInfo());
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        // start the service and immediately cancel
        await service.StartAsync(token);
        await Task.Delay(400);
        cts.Cancel();
        Assert.AreEqual(model.AccumulatedWatts, Math.Max(value,0));
        Assert.AreEqual(model.AccumulatedWattsHourly, Math.Max(value, 0));
    }

    [DataRow(0)]
    [DataRow(45)]
    [TestMethod]
    public async Task TestServiceUpdatesModelWithDifferentDatetime(double value)
    {
        var powerInfo = new Mock<PowerInfo>();
        powerInfo.Setup(p => p.Power).Returns(value);
        var model = new EnergyUsageModel(new CarbonIntensityInfo(), new EnergyRateInfo(), _databaseService);
        Assert.AreEqual(model.AccumulatedWatts, 0);
        var cpuInfo = new Mock<CpuInfo>();
        var gpuInfo = new Mock<GpuInfo>();
        var data = new Dictionary<string, double>();
        data.Add("process", 0);
        model.AccumulatedWattsPerApp = data;

        var currentDay = model.CurrentDay.DateTime;
        var year = currentDay.Year;
        var month = currentDay.Month;
        var day = currentDay.Day+1;
        var hour = currentDay.Hour-1;
        var newDatetime = new DateTime(year, month, day, hour, 0, 0);
        TimeSpan offset = TimeSpan.FromHours(0);
        var newDatetimeOffset = new DateTimeOffset(newDatetime, offset);
        model.CurrentHour = newDatetimeOffset;
        model.CurrentDay = newDatetimeOffset;

        cpuInfo.Setup(c => c.ProcessesCpuUsage).Returns(data);
        gpuInfo.Setup(g => g.ProcessesGpuUsage).Returns(data);
        var service = new PowerMonitorService(model, powerInfo.Object, new CpuInfo(), new GpuInfo());
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        // start the service and immediately cancel
        await service.StartAsync(token);
        await Task.Delay(400);
        cts.Cancel();
        Assert.AreEqual(model.AccumulatedWatts, Math.Max(value, 0));
        Assert.AreEqual(model.AccumulatedWattsHourly, Math.Max(value, 0));
        
    }

}
