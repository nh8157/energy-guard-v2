using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using Newtonsoft.Json;
using Windows.Devices.Geolocation;
using Windows.Media.Protection.PlayReady;

namespace EnergyPerformance.Services;
public class LocationService : ILocationService
{
    private readonly LocationInfo _locationInfo;

    public LocationService()
    {
        _locationInfo = new LocationInfo();
    }

    public async Task<LocationInfo> GetLocationInfo()
    {
        if (_locationInfo.Country == "Unknown")
        {
            await InitializeLocationService();
        }
        return _locationInfo;
    }


    private async Task InitializeLocationService()
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
                    _locationInfo.Country = country;
                    if(country.ToLower() == "united kingdom")
                    {
                        string postcode = jsonResponse["address"]["postcode"];
                        _locationInfo.PostCode = postcode.Split(" ")[0];
                        string state = jsonResponse["address"]["state"];
                        switch (state)
                        {
                            case "England":
                                string district = jsonResponse["address"]["state_district"];
                                _locationInfo.Region = getEnglandRegion(district);
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
        }
    }

    private string getEnglandRegion(string district)
    {
        App.GetService<DebugModel>().AddMessage(district);

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
