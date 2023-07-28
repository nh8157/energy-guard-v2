using System.Net;
using System.Reflection;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using LiveChartsCore.Themes;
using Windows.Foundation.Numerics;

namespace EnergyPerformance.Services;

public class EnergyRateService: IEnergyRateService
{
    private readonly string _countryCodesFileName = "country_codes";
    private readonly string _dnoRegionNumFileName = "dno_region_numbers";
    private readonly string _eurostatYear = "2022";
    private readonly string _voltage = "HV";
    private readonly IHttpClientFactory _httpClientFactory;

    public EnergyRateService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Returns the energy rate of a chosen country. 
    /// The 'ukRegion' parameter is required exclusively 
    /// when the given country is 'United Kingdom'.
    /// </summary>
    public async Task<double> GetEnergyRateAsync(string countryName, string ukRegion="")
    {
        if (countryName.ToLower().Equals("united kingdom"))
        {
            if (string.IsNullOrEmpty(ukRegion))
            {
                throw new ArgumentException("DNO must be provided for United Kingdom energy rate.");
            }
            var dno = GetDNO(ukRegion) ??
                throw new ArgumentException("Please provide a valid UK Region.");

            var energyRateUK = await GetEnergyRateUKAsync(dno);
            return energyRateUK;
        }
        var countryCode = GetCountryCode(countryName) ??
            throw new ArgumentException($"The country {countryName} is not supported.");

        var energyRateEurope = await GetEnergyRateEuropeAsync(countryCode);
        return energyRateEurope;
    }

    /// <summary>
    /// Retrieves the energy rate of the United Kingdom for a specific region, 
    /// requiring the corresponding DNO (Distribution Network Operator) number. 
    /// For further details about the DNO, please visit: 
    /// https://electricitycosts.org.uk/api/
    /// </summary>
    /// 
    private async Task<double> GetEnergyRateUKAsync(int dno)
    {
        var dateNow = DateTime.Now.ToString("dd-MM-yyyy");
        var uriQuery = $"?dno={dno}&voltage={_voltage}&start={dateNow}&end={dateNow}";
        var httpClient = _httpClientFactory.CreateClient("EnergyCostsApi");
        var energyCostsApi = await ApiProcessor<EnergyCostsModel>.Load(httpClient, uriQuery) ??
        throw new Exception("EnergyCosts API is not available.");
        return energyCostsApi.GetEnergyRateUK() / 100;
    }

    /// <summary>
    /// Retrieves the energy rate for European countries, 
    /// excluding the United Kingdom. To utilize this feature, 
    /// you need to provide the two-letter country code of the specific country. 
    /// For a list of country codes, please refer to: 
    /// https://ec.europa.eu/eurostat/statistics-explained/index.php?title=Glossary:Country_codes.
    /// </summary>
    /// 
    private async Task<double> GetEnergyRateEuropeAsync(string countryCode)
    {
        var uriQuery = $"?format=JSON&time={_eurostatYear}";
        var httpClient = _httpClientFactory.CreateClient("EurostatApi");
        var eurostatApi = await ApiProcessor<EurostatModel>.Load(httpClient, uriQuery) ??
        throw new Exception("Eurostat API is not available.");
        return eurostatApi.GetEnergyRate(countryCode);
    }

    /// <summary>
    /// Retrieves the country code of a given country within Europe.
    /// <summary>
    private string? GetCountryCode(string country)
    {
        var countryCode = FindMatch(_countryCodesFileName, country);

        if (string.IsNullOrEmpty(countryCode))
        {
            return null;
        }
        return countryCode.ToUpper();
    }

    /// <summary>
    /// Retrieves the DNO number of a given region within the United Kingdom.
    /// <summary>
    private int? GetDNO(string region)
    {
        var dno = FindMatch(_dnoRegionNumFileName, region);

        if (string.IsNullOrEmpty(dno))
        {
            return null;
        }
        return int.Parse(dno);
    }

    /// <summary>
    /// Depending on the given file name, this matches 
    /// countries with their respective country codes or matches 
    /// United Kingdom regions with their DNO numbers.
    /// <summary>
    private static string FindMatch(string fileName, string str)
    {
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
            throw new Exception($"Cannot find path to file {fileName}.");
        var filePath = Path.Combine(basePath, fileName);

        var matchEqvalent = "";
        using var read = new StreamReader(filePath);

        while (!read.EndOfStream)
        {
            var line = read.ReadLine();
            if (line == null)
            {
                break;
            }
            var values = line.Split(',');

            var column1 = values[0].ToLower();
            var column2 = values[1].ToLower();

            if (column2.ToLower().Equals(str.ToLower()))
            {
                matchEqvalent = column1;
            }
        }
        return matchEqvalent;
    }
}