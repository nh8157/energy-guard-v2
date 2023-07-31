using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using Newtonsoft.Json;
using Windows.Devices.Geolocation;
using Windows.Media.Protection.PlayReady;
using System.Diagnostics.Metrics;

namespace EnergyPerformance.Services;
public class LocationService : BackgroundService, ILocationService
{
    private LocationInfo _locationInfo;
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromHours(1));
    private bool _isInitialzed;
    public string Country
    {
        get => _locationInfo.Country;
        private set => _locationInfo.Country = value;
    }

    public string PostCode
    {
        get => _locationInfo.PostCode;
        private set => _locationInfo.PostCode = value;
    }

    public string Region
    {
        get => _locationInfo.Region;
        private set => _locationInfo.Region = value;
    }


    public LocationService()
    {
        _locationInfo = new LocationInfo();
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
        var accessStatus = await Geolocator.RequestAccessAsync();

        if (accessStatus == GeolocationAccessStatus.Allowed)
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;
            try
            {
                Geoposition pos = await geolocator.GetGeopositionAsync();
                double latitude = pos.Coordinate.Point.Position.Latitude;
                double longitude = pos.Coordinate.Point.Position.Longitude;
                using (HttpClient client = new HttpClient())
                {
                    var url = new Uri($"https://geocode.maps.co/reverse?lat={latitude}&lon={longitude}");
                    var result = client.GetAsync(url).Result;
                    var responseBody = result.Content.ReadAsStringAsync().Result;
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
                    string country = jsonResponse["address"]["country"];
                    Country = country;
                    if (country.ToLower() == "united kingdom")
                    {
                        string postcode = jsonResponse["address"]["postcode"];
                        PostCode = postcode.Split(" ")[0];
                        string state = jsonResponse["address"]["state"];
                        switch (state)
                        {
                            case "England":
                                string district = jsonResponse["address"]["state_district"];
                                Region = getEnglandRegion(district);
                                break;
                            case "Scotland":
                                break;
                            case "Wales":
                                break;
                            case "Ireland":
                                break;
                            default:
                                break;
                        }
                    }

                }
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

    private string getEnglandRegion(string district)
    {
        switch (district)
        {
            case "Greater London":
                return "London";
            case "West Midlands":
                return "Midlands";
            case "East Midlands":
                return "East Midlands";
            case "North East England":
                return "North East";
            case "North West England":
                return "North West";
            case "Yorkshire and the Humber":
                return "Yorkshire";
            case "East of England":
                return "Eastern England";
            case "South East England":
                return "South East";
            case "South West England":
                return "South Western";
            default:
                return "Unknown";
        }
    }
}