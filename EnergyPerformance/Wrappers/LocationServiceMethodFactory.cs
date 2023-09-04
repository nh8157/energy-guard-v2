using Windows.Devices.Geolocation;

namespace EnergyPerformance.Wrapper;
public class LocationServiceMethodFactory
{
    public async virtual Task<GeolocationAccessStatus> RequestAccessAsync()
    {
        return await Geolocator.RequestAccessAsync();
    }


}