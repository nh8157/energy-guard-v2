using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EnergyPerformance.Helpers;
using Microsoft.Extensions.Hosting;

namespace EnergyPerformance.Services;
class CarbonIntensityUpdateService : BackgroundService
{
    private readonly string _ukApiUrl = "https://api.carbonintensity.org.uk/regional/postcode/{0}";
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMinutes(10));
    private readonly CarbonIntensityInfo _carbonIntensityInfo;
    private readonly LocationInfo _locationInfo;

    public double CarbonIntensity
    {
        get => _carbonIntensityInfo.CarbonIntensity;
        set => _carbonIntensityInfo.CarbonIntensity = value;
    }

    public string Country => _locationInfo.Country;

    public string PostCode => _locationInfo.PostCode;

    public CarbonIntensityUpdateService(CarbonIntensityInfo carbonIntensityInfo, LocationInfo locationInfo)
    {
        _carbonIntensityInfo = carbonIntensityInfo;
        _locationInfo = locationInfo;
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
        // Only supports retrieving carbon intensity within the UK
        if (Country == "Great Britain")
        {
            await FetchLiveCarbonIntensity();
        } else
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
}
