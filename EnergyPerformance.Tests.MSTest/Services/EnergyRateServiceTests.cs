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
public class EnergyRateServiceTests
{
    [ClassInitialize]
    public static void ClassInitialze(TestContext context)
    {
        Debug.WriteLine("Initialize");
    }

    public EnergyRateService GetService()
    {
        return new EnergyRateService(new LocationInfo(), new EnergyRateInfo());
    }

    [TestMethod]
    public void TestEnergyRateService()
    {
        var service = GetService();
        Assert.IsNotNull(service);
    }


    [DataRow("United Kingdom", "WC1E 6BT")]
    [DataRow("Greece", "")]
    [DataRow("France", "")]
    [TestMethod]
    public async Task TestEnergyRateUpdatedSuccess(string value1, string value2)
    {
        var locationInfo = new Mock<LocationInfo>();
        locationInfo.Setup(l => l.Country).Returns(value1);
        locationInfo.Setup(l => l.Postcode).Returns(value2);
        var energyRateInfo = new EnergyRateInfo();
        var service = new EnergyRateService(locationInfo.Object, energyRateInfo);
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        await service.StartAsync(token);
        await Task.Delay(3000);
        cts.Cancel();
        Assert.AreNotEqual(energyRateInfo.EnergyRate, 0);
    }

}
