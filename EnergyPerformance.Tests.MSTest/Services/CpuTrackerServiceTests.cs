using CLI;
using EnergyPerformance.Core.Contracts.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnergyPerformance.Services;
using EnergyPerformance.Core.Services;
using EnergyPerformance.Models;
using EnergyPerformance.ViewModels;
using Microsoft.Extensions.Options;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;
using Moq;

namespace EnergyPerformance.Tests.MSTest.Services;

[TestClass()]
public class CpuTrackerServiceTests
{
    private static IFileService _fileService;
    private static IOptions<LocalSettingsOptions> localSettingsOptions;
    private static Mock<IAppNotificationService> _notificationService;
    private const int totalCores = 16;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _fileService = new FileService();
        _notificationService = new Mock<IAppNotificationService>();
        localSettingsOptions = Options.Create(new LocalSettingsOptions());
    }


    public CpuTrackerService GetService()
    {
        var localSettingsService = new LocalSettingsService(_fileService, localSettingsOptions);
        var cpuTrackerService = new CpuTrackerService(localSettingsService, _notificationService.Object, new CpuInfo());
        return  cpuTrackerService;
    }

    [TestMethod()]
    public void CpuTrackerServiceTest()
    {
        var service = GetService();
        var controller = new Controller();
        if (controller.PerformanceCoreCount() >= 2)
        {
            Assert.IsTrue(service.SupportedCpu);
        }
        else
        {
            Assert.IsFalse(service.SupportedCpu);
        }

    }


    [TestMethod()]
    [DataRow(25, 0)]
    [DataRow(60, 1)]
    [DataRow(90, 2)]
    public async Task AutoControlTest(double value, double actual)
    {
        var service = GetService();
        for (var i = 0; i <= CpuTrackerService.Duration+1; i++)
        {
            await service.AutomaticModeControl(0, totalCores, value);
        }
        Assert.AreEqual(service.CurrentMode, actual);
    }

    [TestMethod()]
    [DataRow(23, 40)]
    [DataRow(16, 60)]
    [DataRow(9, 100)]
    public async Task CasualModeToWorkModeTest(double value1, double value2)
    {
        var service = GetService();
        await service.AutomaticModeControl(0, totalCores, value1);
        Assert.AreEqual(service.CurrentMode, 0);
        await service.AutomaticModeControl(4, totalCores, value2);
        Assert.AreEqual(service.CurrentMode, 1);
    }


    [TestMethod()]
    [DataRow(34, 54)]
    [DataRow(68, 78)]
    [DataRow(55, 100)]
    public async Task WorkModeToPerformanceModeTest(double value1, double value2)
    {
        var service = GetService();
        await service.AutomaticModeControl(0, totalCores, value1);
        Assert.AreEqual(service.CurrentMode, 1);
        await service.AutomaticModeControl(8, totalCores, value2);
        Assert.AreEqual(service.CurrentMode, 2);
    }


    [TestMethod()]
    [DataRow(78, 31)]
    [DataRow(96, 48)]
    [DataRow(100, 36)]
    public async Task PerformanceModeToWorkModeTest(double value1, double value2)
    {
        var service = GetService();
        await service.AutomaticModeControl(0, totalCores, value1);
        Assert.AreEqual(service.CurrentMode, 2);
        await service.AutomaticModeControl(0, totalCores, value2);
        Assert.AreEqual(service.CurrentMode, 1);
    }


    [TestMethod()]
    [DataRow(56, 8)]
    [DataRow(67, 6)]
    [DataRow(31, 3)]
    public async Task WorkModeToCasualModeTest(double value1, double value2)
    {
        var service = GetService();
        await service.AutomaticModeControl(0, totalCores, value1);
        Assert.AreEqual(service.CurrentMode, 1);
        await service.AutomaticModeControl(8, totalCores, value2);
        Assert.AreEqual(service.CurrentMode, 0);
    }



}