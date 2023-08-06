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
public class CarbonIntensityUpdateServiceTest
{
    [ClassInitialize]
    public static void ClassInitialze(TestContext context)
    {
        Debug.WriteLine("Initialize");
    }

    public CarbonIntensityUpdateService GetService()
    {
        return new CarbonIntensityUpdateService(new CarbonIntensityInfo(), new LocationInfo());
    }

    [TestMethod]
    public void TestCarbonIntensityUpdateService()
    {
        var service = GetService();
        Assert.IsNotNull(service);
    }

    [DataRow("United Kingdom","N1")]
    [DataRow("United Kingdom","N1 Abc")]
    [TestMethod]
    public async Task TestCarbonIntensityUpdatedSuccess(string value1, string value2)
    {
        var locationInfo = new Mock<LocationInfo>();
        locationInfo.Setup(l => l.Country).Returns(value1);
        locationInfo.Setup(l => l.Postcode).Returns(value2);
        var carbonIntensityInfo = new CarbonIntensityInfo();
        var service = new CarbonIntensityUpdateService(carbonIntensityInfo, locationInfo.Object);
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        await service.StartAsync(token);
        await Task.Delay(1000);
        cts.Cancel();
        Assert.AreNotEqual(carbonIntensityInfo.CarbonIntensity, 100);
    }

}
