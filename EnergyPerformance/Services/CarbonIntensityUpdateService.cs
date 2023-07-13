using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EnergyPerformance.Helpers;
using Microsoft.Extensions.Hosting;
using EnergyPerformance.Models;
using EnergyPerformance.Core.Contracts.Services;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using System.Diagnostics.Metrics;
using System.Globalization;
using IPinfo;
using IPinfo.Models;
using System.Net;
using EnergyPerformance.Contracts.Services;
using System.Data.SQLite;
using System.Data.Entity;
using Microsoft.SqlServer.Server;
using EnergyPerformance.Core.Helpers;

namespace EnergyPerformance.Services;
class CarbonIntensityUpdateService : BackgroundService, ICarbonIntensityUpdateService
{
    private readonly string _ukApiUrl = "https://api.carbonintensity.org.uk/regional/postcode/{0}";
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMinutes(10));
    private readonly CarbonIntensityInfo _carbonIntensityInfo;
    private LocationInfo _locationInfo;
    private readonly IFileService _fileService;
    private const string _defaultApplicationDataFolder = "EnergyPerformance/ApplicationData";
    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string IPInfoToken;
    private string ip;
    private readonly IPinfoClient client;

    public double CarbonIntensity
    {
        get => _carbonIntensityInfo.CarbonIntensity;
        set => _carbonIntensityInfo.CarbonIntensity = value;
    }

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

    public LocationInfo LocationInfo { 
        get => _locationInfo;
        private set => _locationInfo = value;
    }

    public CarbonIntensityUpdateService(CarbonIntensityInfo carbonIntensityInfo, IFileService fileService)
    {
        _carbonIntensityInfo = carbonIntensityInfo;
        _locationInfo = new LocationInfo();
        _fileService = fileService;
        IPInfoToken = "45f7eb1ccaffe7";
        client = new IPinfoClient.Builder()
        .AccessToken(IPInfoToken)
        .Build();
        ip = string.Empty;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            await DoAsync();
        }
        while (await _periodicTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
    }

    private async Task DoAsync()
    {
        Debug.WriteLine("Retrieving live carbon intensity");
        if (ip == null)
        {
            ip = await GetIPAddress();
        }

        if (_locationInfo.Country=="Unknown"|| _locationInfo.PostCode=="Unknown")
        {
            await GetLocation();
        }
        if (Country == "United Kingdom")
        {
            await FetchLiveCarbonIntensity();
        }
        else
        {
            Debug.WriteLine("Other countries and regions are currently not supported");
        }
        Debug.WriteLine($"Current carbon intensity: {_carbonIntensityInfo.CarbonIntensity}");
    }

    private async Task FetchLiveCarbonIntensity()
    {
        var client = new HttpClient();
        try
        {
            var url = String.Format(_ukApiUrl, PostCode);
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
                // Debug.WriteLine(responseBody);
                JArray data = jsonResponse.data;

                foreach (var d in data)
                {
                    JArray data_data = (JArray)d["data"];
                    foreach (var dp in data_data)
                    {
                        JToken intensity = dp["intensity"];
                        CarbonIntensity = (double)intensity["forecast"];
                    }
                }
            }
            else
            {
                Debug.WriteLine("Request failed");
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Cannot fetch data", e);
        }
    }
    public async Task GetLocation()
    {
        IPResponse ipResponse = await client.IPApi.GetDetailsAsync(ip);
        string country = ipResponse.CountryName;
        string postal = ipResponse.Postal;
        App.GetService<DebugModel>().AddMessage("Location Updated");
        Country = country;
        PostCode = postal;
    }

    static async Task<string> GetIPAddress()
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
