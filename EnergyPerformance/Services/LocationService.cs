using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using Newtonsoft.Json;
using Windows.Devices.Geolocation;
using System.Text.Json;

namespace EnergyPerformance.Services;
public class LocationService : BackgroundService
{
    private readonly LocationInfo _locationInfo;
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromHours(1));
    private const string _locationUrl = "https://geocode.maps.co/reverse?lat={0}&lon={1}";

    public string Country
    {
        get => _locationInfo.Country;
        private set => _locationInfo.Country = value;
    }

    public string Postcode
    {
        get => _locationInfo.Postcode;
        private set => _locationInfo.Postcode = value;
    }


    public LocationService(LocationInfo locationInfo)
    {
        _locationInfo = locationInfo;
    }

    protected async override Task ExecuteAsync(CancellationToken token)
    {
        do
        {
            await DoAsync();
            App.GetService<DebugModel>().AddMessage(Postcode);

        }
        while (await _periodicTimer.WaitForNextTickAsync(token) && !token.IsCancellationRequested);
    }

    private async Task DoAsync()
    {
        var accessStatus = await Geolocator.RequestAccessAsync();

        if (accessStatus == GeolocationAccessStatus.Allowed)
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;
            try
            {
                // retrieve coordiates from Geoposition
                Geoposition pos = await geolocator.GetGeopositionAsync();
                var latitude = pos.Coordinate.Point.Position.Latitude;
                var longitude = pos.Coordinate.Point.Position.Longitude;

                HttpClient client = new HttpClient();
                string url = string.Format(_locationUrl, latitude, longitude);
                JsonElement jsonResponse = await ApiProcessor<dynamic>.Load(client, url)??
                    throw new InvalidOperationException("Cannot deserialize json object"); 
                Country = jsonResponse.GetProperty("address").GetProperty("country").ToString();
                Postcode = jsonResponse.GetProperty("address").GetProperty("postcode").ToString();
            }
            catch (Exception ex)
            {
                App.GetService<DebugModel>().AddMessage(ex.ToString());
            }
        }
        else
        {
            Debug.WriteLine("Please Enable Access To Geolocation");
            // request access permission
        }
        await Task.CompletedTask;
    }
}