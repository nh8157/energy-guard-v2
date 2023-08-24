using System.Diagnostics;
using EnergyPerformance.Helpers;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace EnergyPerformance.Services;
class CarbonIntensityUpdateService : BackgroundService
{
    private readonly string _ukUrl = "https://api.carbonintensity.org.uk/regional/postcode/{0}";
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMinutes(5));
    private readonly CarbonIntensityInfo _carbonIntensityInfo;
    private readonly LocationInfo _locationInfo;
    private readonly IHttpClientFactory _httpClientFactory;

    public double CarbonIntensity
    {
        get => _carbonIntensityInfo.CarbonIntensity;
        set => _carbonIntensityInfo.CarbonIntensity = value;
    }

    public string Country => _locationInfo.Country;
    public string Postcode => _locationInfo.Postcode;

    public CarbonIntensityUpdateService(CarbonIntensityInfo carbonIntensityInfo, LocationInfo locationInfo, IHttpClientFactory httpClientFactory)
    {
        _carbonIntensityInfo = carbonIntensityInfo;
        _locationInfo = locationInfo;
        _httpClientFactory = httpClientFactory;
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
        Debug.WriteLine($"Retrieving live carbon intensity for {Country}");

        if (Country.ToLower() == "united kingdom")
        {
            await FetchLiveCarbonIntensity();
        }
        else
        {
            Debug.WriteLine("Other countries and regions are currently not supported");
        }
        Debug.WriteLine($"Current carbon intensity: {CarbonIntensity}");
    }

    private async Task FetchLiveCarbonIntensity()
    {
        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            var url = string.Format(_ukUrl, Postcode.Split(" ")[0]);

            JsonElement jsonResponse = await ApiProcessor<dynamic>.Load(httpClient, url) ??
                throw new InvalidOperationException("Cannot deserialize object");

            var jsonData = jsonResponse.GetProperty("data");
            foreach (var entry in jsonData.EnumerateArray())
            {
                var data = entry.GetProperty("data");
                foreach (var detailedData in data.EnumerateArray())
                {
                    CarbonIntensity = detailedData.GetProperty("intensity").GetProperty("forecast").GetDouble();
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Cannot fetch data", e);
        }
    }

}