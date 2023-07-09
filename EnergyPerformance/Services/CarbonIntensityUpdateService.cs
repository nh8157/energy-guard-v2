using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EnergyPerformance.Helpers;
using Microsoft.Extensions.Hosting;
using EnergyPerformance.Models;
using EnergyPerformance.Core.Contracts.Services;

namespace EnergyPerformance.Services;
class CarbonIntensityUpdateService : BackgroundService
{
    private readonly string _ukApiUrl = "https://api.carbonintensity.org.uk/regional/postcode/{0}";
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMinutes(10));
    private readonly CarbonIntensityInfo _carbonIntensityInfo;
    private LocationInfo _locationInfo;
    private readonly IFileService _fileService;
    private const string _defaultApplicationDataFolder = "EnergyPerformance/ApplicationData";
    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public double CarbonIntensity
    {
        get => _carbonIntensityInfo.CarbonIntensity;
        set => _carbonIntensityInfo.CarbonIntensity = value;
    }

    public string Country => _locationInfo.Country;
    public string PostCode => _locationInfo.PostCode;


    public CarbonIntensityUpdateService(CarbonIntensityInfo carbonIntensityInfo, LocationInfo locationInfo,IFileService fileService)
    {
        _carbonIntensityInfo = carbonIntensityInfo;
        _locationInfo = new LocationInfo();
        _fileService = fileService;
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
        if(_locationInfo.Country!="Unknown"&& _locationInfo.PostCode!="Unknown")
        {
            if (Country == "Great Britain")
            {
                await FetchLiveCarbonIntensity();
            }
            else
            {
                Debug.WriteLine("Other countries and regions are currently not supported");
            }
        }
        // Only supports retrieving carbon intensity within the UK
        else
        {
            App.GetService<DebugModel>().AddMessage("Getting Location");
            await GetLocation();
        }
        Debug.WriteLine($"Current carbon intensity: {_carbonIntensityInfo.CarbonIntensity}");
        App.GetService<DebugModel>().AddMessage($"Current carbon intensity: {_carbonIntensityInfo.CarbonIntensity}");
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
        string _applicationDataFolder = Path.Combine(_localApplicationData, _defaultApplicationDataFolder);
        if (File.Exists(Path.Combine(_applicationDataFolder, "Location.json")))
        {
            App.GetService<DebugModel>().AddMessage("Read Location From Local Storage");
            _locationInfo = await Task.Run(() => _fileService.Read<LocationInfo>(_applicationDataFolder, "Location.json"));
        }
        else
        {
            //add country and postcode
            await UpdateLocation("Great Britain", "WC1E");
        }
        App.GetService<DebugModel>().AddMessage("Location Updated");
    }
    private async Task UpdateLocation(string country, string postcode)
    {
        string _applicationDataFolder = Path.Combine(_localApplicationData, _defaultApplicationDataFolder);
        if (File.Exists(Path.Combine(_applicationDataFolder, "Location.json")))
        {
            await Task.Run(() => _fileService.Delete(_applicationDataFolder, "Location.json"));
        }
        App.GetService<DebugModel>().AddMessage("here");
        _locationInfo = new LocationInfo(country, postcode);
        App.GetService<DebugModel>().AddMessage("Store Location To Local");
        await Task.Run(() => _fileService.Save<LocationInfo>(_applicationDataFolder, "Location.json", _locationInfo));
    }
    
}
