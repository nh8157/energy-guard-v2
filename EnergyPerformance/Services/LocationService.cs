using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using Newtonsoft.Json;
using Windows.Devices.Geolocation;
using System.Text.Json;
using EnergyPerformance.Wrapper;

namespace EnergyPerformance.Services;
public class LocationService : BackgroundService
{
    private const string _locationUrl = "https://geocode.maps.co/reverse?lat={0}&lon={1}";
    private const int duration = 1;
    private readonly Geolocator _geolocator;
    private readonly LocationInfo _locationInfo;
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromHours(duration));
    private readonly IHttpClientFactory _httpClientFactory;
    
    public GeolocationAccessStatus GeolocationAccessStatus
    {
        get; set;
    }

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

    public LocationServiceMethodFactory MethodsWrapper
    {
        get; set;
    }

    public LocationService(LocationInfo locationInfo, IHttpClientFactory httpClientFactory)
    {
        _locationInfo = locationInfo;
        _geolocator = new Geolocator();
        MethodsWrapper = new LocationServiceMethodFactory();
        _httpClientFactory = httpClientFactory;
    }


    protected async override Task ExecuteAsync(CancellationToken token)
    {
        do
        {
            await DoAsync();
        }
        while (await _periodicTimer.WaitForNextTickAsync(token) && !token.IsCancellationRequested);
    }

    private async Task DoAsync()
    {
        GeolocationAccessStatus = await MethodsWrapper.RequestAccessAsync();

        if (GeolocationAccessStatus == GeolocationAccessStatus.Allowed)
        {
            _geolocator.DesiredAccuracyInMeters = 50;
            try
            {
                // retrieve coordiates from Geoposition
                Geoposition pos = await _geolocator.GetGeopositionAsync();
                var latitude = pos.Coordinate.Point.Position.Latitude;
                var longitude  = pos.Coordinate.Point.Position.Longitude;

                HttpClient client = _httpClientFactory.CreateClient();

                var url = string.Format(_locationUrl, latitude, longitude);
                JsonElement jsonResponse = await ApiProcessor<dynamic>.Load(client, url) ??
                    throw new InvalidOperationException("Cannot deserialize json object");

                try
                {
                    Country = jsonResponse.GetProperty("address").GetProperty("country").ToString();
                }
                catch (KeyNotFoundException)
                {
                    Country = "Unknown";
                }

                try
                {
                    Postcode = jsonResponse.GetProperty("address").GetProperty("postcode").ToString();
                }
                catch (KeyNotFoundException)
                {
                    Postcode = "Unknown";
                }
            }
            catch (Exception ex)
            {
            }
        }
        else
        {
            Debug.WriteLine("Please Enable Access To Geolocation");
            Country = "Unavailable";
            Postcode = "Unavailable";
            // request access permission
        }
        await Task.CompletedTask;
    }
}