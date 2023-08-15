using System.Diagnostics;
using EnergyPerformance.Helpers;
using EnergyPerformance.Services;
using EnergyPerformance.Wrapper;
using Moq;
using Windows.Devices.Geolocation;

namespace EnergyPerformance.Tests.MSTest.Services;
[TestClass()]
public class LocationServiceTests
{
    [ClassInitialize]
    public static void ClassInitialze(TestContext context)
    {
        Debug.WriteLine("Class initialize");
    }

    public LocationService GetService()
    {
        return new LocationService(new LocationInfo());
    }

    [TestMethod]
    public void TestLocationService()
    {
        var service = GetService();
        Assert.IsNotNull(service);
    }


    [TestMethod]
    public async Task TestLocationInfoUpdateWithGeoLocationAccess()
    {
        var methodWrapper = new Mock<LocationServiceMethodFactory>();
        methodWrapper.Setup(m => m.RequestAccessAsync()).ReturnsAsync(GeolocationAccessStatus.Allowed);
        var locationInfo = new LocationInfo();
        var locationService = new LocationService(locationInfo);
        locationService.MethodsWrapper = methodWrapper.Object;
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        await locationService.StartAsync(token);
        await Task.Delay(4000);
        cts.Cancel();
        Assert.AreNotEqual(locationInfo.Country, "Unknown");
    }

    [TestMethod]
    public async Task TestLocationUpdateWithoutGeoLocationAccess()
    {
        var methodWrapper = new Mock<LocationServiceMethodFactory>();
        methodWrapper.Setup(m => m.RequestAccessAsync()).ReturnsAsync(GeolocationAccessStatus.Denied);
        var locationInfo = new LocationInfo();
        var locationService = new LocationService(locationInfo);
        locationService.MethodsWrapper = methodWrapper.Object;
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        await locationService.StartAsync(token);
        await Task.Delay(1000);
        cts.Cancel();
        Assert.AreEqual(locationInfo.Country, "Unavailable");
    }
    
}
