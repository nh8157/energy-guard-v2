using EnergyPerformance.Services;
using Moq;
using EnergyPerformance.Models;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Services;
using EnergyPerformance.Core.Contracts.Services;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using EnergyPerformance.Helpers;
using EnergyPerformance.Tests.MSTest;
using EnergyPerformance.Core.Helpers;
using System.ComponentModel;

namespace EnergyPerformance.ViewModels.Tests;

[TestClass()]
public class MainViewModelTests
{
    private static Mock<ILocalSettingsService> _localSettingsService;
    private static Mock<IAppNotificationService> _appNotificationService;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _appNotificationService = new Mock<IAppNotificationService>();
        _localSettingsService = new Mock<ILocalSettingsService>();
    }

    [TestMethod()]
    [DataRow(7)]
    [DataRow(14)]
    [DataRow(21)]
    public async Task TestGetCost(int value)
    {
        var diaries = DataTestClass.GenerateListOfRandomEnergyDiaries(value);
        var data = new EnergyUsageData(1.0, 0.5, diaries);
        Assert.IsNotNull(data);
        var databaseService = new Mock<IDatabaseService>();
        databaseService.Setup(d => d.LoadUsageData()).ReturnsAsync(data);
        var energyUsageModel = new EnergyUsageModel(new CarbonIntensityInfo(), new EnergyRateInfo(), databaseService.Object);
        await energyUsageModel.InitializeAsync();
        var model = new MainViewModel(new PowerInfo(), new CpuInfo(), _localSettingsService.Object,
            _appNotificationService.Object, energyUsageModel);
        Assert.IsNotNull(model.CostPreviousWeek);
        Assert.AreEqual(model.CostPreviousWeek, energyUsageModel.GetCostForPreviousWeek());
        Assert.IsNotNull(model.CostThisWeek);
        Assert.AreEqual(model.CostThisWeek, energyUsageModel.GetCostForCurrentWeek());
    }

    [TestMethod()]
    public async Task TestSelectMode()
    {
        var settingsService = new LocalSettingsService(new Mock<IFileService>().Object, Options.Create(new LocalSettingsOptions()));
        var model = new MainViewModel(new PowerInfo(), new CpuInfo(), settingsService,
            _appNotificationService.Object, new EnergyUsageModel(new CarbonIntensityInfo(), new EnergyRateInfo(), new DatabaseService("testDB.db")));
        await model.SelectAutoControl();
        Assert.AreEqual(settingsService.SelectedMode, model.SelectedMode);
        await model.SelectCasualMode();
        Assert.AreEqual(settingsService.SelectedMode, model.SelectedMode);
        await model.SelectWorkMode();
        Assert.AreEqual(settingsService.SelectedMode, model.SelectedMode);
        await model.SelectPerformanceMode();
        Assert.AreEqual(settingsService.SelectedMode, model.SelectedMode);
    }

    [TestMethod()]
    [DataRow(1)]
    [DataRow(0.3)]
    [DataRow(0)]
    public void TestPowerInfoChange(double value)
    {
        var powerInfo = new PowerInfo();
        var model = new MainViewModel(powerInfo, new CpuInfo(), _localSettingsService.Object,
            _appNotificationService.Object, new EnergyUsageModel(new CarbonIntensityInfo(), new EnergyRateInfo(), new DatabaseService("testDB.db")));
        powerInfo.Power = value;
        Assert.AreEqual(model.Power, value);
    }

    [TestMethod()]
    [DataRow(0.5)]
    [DataRow(2)]
    [DataRow(0)]
    public void TestCpuUsageChange(double value)
    {
        var cpuInfo = new CpuInfo();
        var model = new MainViewModel(new PowerInfo(), cpuInfo, _localSettingsService.Object,
            _appNotificationService.Object, new EnergyUsageModel(new CarbonIntensityInfo(), new EnergyRateInfo(), new DatabaseService("testDB.db")));
        cpuInfo.CpuUsage = value;
        Assert.AreEqual(model.CpuUsage, value);
    }

    [TestMethod()]
    [DataRow(true, false)]
    [DataRow(true, true)]
    [DataRow(false, false)]
    public void TestAutoControlChange(bool value1, bool value2)
    {
        var cpuInfo = new CpuInfo();
        var settingsService = new LocalSettingsService(new Mock<IFileService>().Object, Options.Create(new LocalSettingsOptions()));
        var model = new MainViewModel(new PowerInfo(), cpuInfo, settingsService,
            _appNotificationService.Object, new EnergyUsageModel(new CarbonIntensityInfo(), new EnergyRateInfo(), new DatabaseService("testDB.db")));
        cpuInfo.IsSupported = value1;
        settingsService.AutoControlSetting = value2;
        Assert.AreEqual(model.AutoControl, value1&&value2);
    }
}
