using Moq;
using EnergyPerformance.Models;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Services;
using EnergyPerformance.Core.Contracts.Services;
using EnergyPerformance.Core.Helpers;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using EnergyPerformance.Helpers;
using System.Data.Entity;
using EnergyPerformance.Services;
using System.Diagnostics;

namespace EnergyPerformance.Models.Tests;
[TestClass()]
public class EnergyUsageModelTests
{

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        Debug.WriteLine("ClassInit");
    }

    private static double RandomDouble(double min, double max)
    {
        // Generate a random number between min and max
        Random random = new Random();
        return (random.NextDouble() * (max - min) + min);
    }

    private static EnergyUsageData generateRandomUsageData(float power, int hourOffset)
    {
        var diaries = new List<EnergyUsageDiary>();
        var datetime = DateTime.Now;
        datetime = datetime.AddHours(hourOffset);
        var randomLog = new EnergyUsageLog(datetime, power, power, power);
        var hourlyLog = new List<EnergyUsageLog>();
        var procLog = new Dictionary<string, EnergyUsageLog>();
        hourlyLog.Add(randomLog);
        procLog.Add("name", randomLog);
        diaries.Add(new EnergyUsageDiary(datetime, randomLog, hourlyLog, procLog));
        var energyUsageData = new EnergyUsageData(1, 1, diaries);
        return energyUsageData;
    }


    [TestMethod()]
    public async Task TestModelIsInitializedCase1()
    {
        var databaseService = new Mock<IDatabaseService>();
        databaseService.Setup(d => d.LoadUsageData()).ReturnsAsync(new EnergyUsageData());
        var model = new EnergyUsageModel(new CarbonIntensityInfo(), new EnergyRateInfo(), databaseService.Object);
        await model.InitializeAsync();
        Assert.AreEqual(model.AccumulatedWatts, 0);
        Assert.AreEqual(model.AccumulatedWattsHourly, 0);
    }

    [TestMethod()]
    public async Task TestModelIsInitializedCase2()
    {
        var databaseService = new Mock<IDatabaseService>();
        var power = RandomDouble(0.5, 2.0);
        var energyUsageData = generateRandomUsageData((float)power, 0);
        databaseService.Setup(d => d.LoadUsageData()).ReturnsAsync(energyUsageData);
        var model = new EnergyUsageModel(new CarbonIntensityInfo(), new EnergyRateInfo(), databaseService.Object);
        await model.InitializeAsync();
        double tolerance = 1e-5;

        Assert.AreEqual((float)model.AccumulatedWatts / 3600000, (float)power, tolerance);
        Assert.AreEqual((float)model.AccumulatedWattsHourly / 3600000, (float)power,tolerance);
    }

    [TestMethod()]
    public async Task TestModelIsInitializedCase3()
    {
        var databaseService = new Mock<IDatabaseService>();
        var power = RandomDouble(0.5, 2.0);
        var energyUsageData = generateRandomUsageData((float)power, -2);
        databaseService.Setup(d => d.LoadUsageData()).ReturnsAsync(energyUsageData);
        var model = new EnergyUsageModel(new CarbonIntensityInfo(), new EnergyRateInfo(), databaseService.Object);
        await model.InitializeAsync();
        double ws = (float)power * 3600000;
        double tolerance = 1e-5;

        Assert.AreEqual((float)model.AccumulatedWatts / 3600000, (float)power, tolerance);
        Assert.AreEqual((float)model.AccumulatedWattsHourly / 3600000, 0);
    }

    [TestMethod()]
    public async Task TestModelUpdate()
    {
        var databaseService = new DatabaseService("testDB.db");
        await databaseService.ClearAllData();
        var model = new Mock< EnergyUsageModel>(new CarbonIntensityInfo(), new EnergyRateInfo(), databaseService);
        var preData = await databaseService.LoadUsageData();
        var power = RandomDouble(0.5, 2.0);
        model.Setup(m => m.AccumulatedWatts).Returns(power);
        await model.Object.Save();
        var curData = await databaseService.LoadUsageData();
        double tolerance = 1e-12;
        Assert.AreEqual((float)curData.Diaries[^1].DailyUsage.PowerUsed, (float)power / 3600000, tolerance);
        await databaseService.ClearAllData();

    }
}
