using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EnergyPerformance.Helpers;
using Microsoft.Extensions.Hosting;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Models;

namespace EnergyPerformance.Services;
public class CarbonIntensityUpdateService : BackgroundService, ICarbonIntensityUpdateService
{
    private readonly string _ukApiUrl = "https://api.carbonintensity.org.uk/regional/postcode/{0}";
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMinutes(5));
    private readonly CarbonIntensityInfo _carbonIntensityInfo;
    private readonly LocationInfo _locationInfo;

    public double CarbonIntensity
    {
        get => _carbonIntensityInfo.CarbonIntensity;
        set => _carbonIntensityInfo.CarbonIntensity = value;
    }

    public string Country => _locationInfo.Country;
    public string Postcode => _locationInfo.Postcode;

    public CarbonIntensityUpdateService(CarbonIntensityInfo carbonIntensityInfo, LocationInfo locationInfo)
    {
        _carbonIntensityInfo = carbonIntensityInfo;
        _locationInfo = locationInfo;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
            await DoAsync();
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
        var client = new HttpClient();
        try
        {
            var url = String.Format(_ukApiUrl, Postcode.Split(" ")[0]);
            
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody)??
                    throw new InvalidOperationException("Cannot deserialize object");
                JArray data = jsonResponse.data??
                    throw new InvalidDataException("'data' field does not exist in jsonResponse");

                foreach (var regionData in data)
                {
                    var detailedData = (JArray?)regionData["data"]??
                        throw new InvalidOperationException("'data' field does not exist");

                    foreach (var entry in detailedData)
                    {
                        JToken carbonInfo = entry["intensity"]??
                            throw new InvalidOperationException("'intensity' field does not exist");
                        var carbonIntensity = (double?)carbonInfo["forecast"];

                        if (carbonIntensity != null)
                            CarbonIntensity = (double)carbonIntensity;
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