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
    private static IOptions<LocalSettingsOptions> localSettingsOptions;
    private static Mock<IAppNotificationService> _notificationService;
    private const int totalCores = 16;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _notificationService = new Mock<IAppNotificationService>();
        localSettingsOptions = Options.Create(new LocalSettingsOptions());
    }


    public CpuTrackerService GetService()
    {
        var cpuTrackerService = new CpuTrackerService(new CpuInfo());
        return  cpuTrackerService;
    }

    // [TestMethod()]
    // public void CpuTrackerServiceTest()
    // {
    //     var service = GetService();
    //     var controller = new Controller();
    //     if (controller.PerformanceCoreCount() >= 2)
    //     {
    //         Assert.IsTrue(service.SupportedCpu);
    //     }
    //     else
    //     {
    //         Assert.IsFalse(service.SupportedCpu);
    //     }
    //
    // }

}