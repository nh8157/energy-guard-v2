using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using IPinfo;
using IPinfo.Models;
using Windows.Media.Protection.PlayReady;

namespace EnergyPerformance.Services;
public class LocationService : ILocationService
{
    private LocationInfo _locationInfo;
    private readonly string _IPInfoToken;
    private string _ip;
    private readonly IPinfoClient _client;

    public string Country
    {
        get => _locationInfo.Country;
        set => _locationInfo.Country = value;
    }

    public string PostCode
    {
        get => _locationInfo.PostCode;
        set => _locationInfo.PostCode = value;
    }

    public LocationInfo LocationInfo
    {
        get => _locationInfo;
        private set => _locationInfo = value;
    }

    public LocationService()
    {
        _locationInfo = new LocationInfo();
        _IPInfoToken = "45f7eb1ccaffe7";
        _client = new IPinfoClient.Builder()
        .AccessToken(_IPInfoToken)
        .Build();
        _ip = string.Empty;
    }

    public async Task<LocationInfo> GetLocationInfo()
    {
        if (_locationInfo.Country == "Unknown" || _locationInfo.PostCode == "Unknown")
        {
            await retrieveLocation();
        }
        return _locationInfo;
    }

    private async Task retrieveLocation()
    {
        if (_ip == null)
        {
            _ip = await RetrieveIPAddress();
        }
        IPResponse ipResponse = await _client.IPApi.GetDetailsAsync(_ip);
        string country = ipResponse.CountryName;
        string postal = ipResponse.Postal;
        App.GetService<DebugModel>().AddMessage("Location Updated");
        _locationInfo.Country = country;
        _locationInfo.PostCode = postal;
    }

    private async Task<string> RetrieveIPAddress()
    {
        string apiUrl = "https://ipinfo.io/ip";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                string ipAddress = await response.Content.ReadAsStringAsync();

                return ipAddress.Trim();
            }
        }
        return string.Empty;
    }
}
