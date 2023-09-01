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
public class GpuTrackerServiceTests
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


    public GpuTrackerService GetService()
    {
        var gpuTrackerService = new GpuTrackerService(new GpuInfo(), new DebugModel());
        return gpuTrackerService;
    }

    [TestMethod()]
    public async Task GpuTrackerServiceTest()
    {
        var service = GetService();
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        await service.StartAsync(token);
        cts.Cancel();
        Assert.AreEqual(service.GpuUsage, 0);
    }

}
