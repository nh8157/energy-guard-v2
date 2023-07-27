using System.Reflection;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;

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

    public async Task<double> GetEnergyRate(string countryName, string ukRegion="")
    {
        if (countryName.ToLower().Equals("united kingdom"))
        {
            if (string.IsNullOrEmpty(ukRegion))
            {
                throw new ArgumentException("DNO must be provided for United Kingdom energy rate.");
            }
            var dno = GetDNO(ukRegion) ??
                throw new ArgumentException("Please provide a valid UK Region.");

            var energyRateUK = await GetEnergyRateUK(dno);
            return energyRateUK;
        }
        var countryCode = GetCountryCode(countryName) ??
            throw new ArgumentException($"The country {countryName} is not supported.");

        var energyRateEurope = await GetEnergyRateEurope(countryCode);
        return energyRateEurope;
    }

    private async Task<double> GetEnergyRateUK(int dno)
    {
        var dateNow = DateTime.Now.ToString("dd-MM-yyyy");
        var uriQuery = $"?dno={dno}&voltage={_voltage}&start={dateNow}&end={dateNow}";

        var httpClient = _httpClientFactory.CreateClient("EnergyCostsApi");

        var energyCostsApi = await ApiProcessor<EnergyCostsModel>.Load(httpClient, uriQuery) ??
            throw new Exception("EnergyCosts API is not available.");

        return energyCostsApi.GetEnergyRateUK() / 100;
    }

    private async Task<double> GetEnergyRateEurope(string countryCode)
    {
        var uriQuery = $"?format=JSON&time={_eurostatYear}";
        var httpClient = _httpClientFactory.CreateClient("EurostatApi");

        var eurostatApi = await ApiProcessor<EurostatModel>.Load(httpClient, uriQuery) ??
            throw new Exception("Eurostat API is not available.");

        return eurostatApi.GetEnergyRate(countryCode);
    }

    private string? GetCountryCode(string country)
    {
        var countryCode = FindMatchFetchSecondColumn(_countryCodesFileName, country);

        if (string.IsNullOrEmpty(countryCode))
        {
            return null;
        }
        return countryCode.ToUpper();
    }

    private int? GetDNO(string region)
    {
        var dno = FindMatchFetchSecondColumn(_dnoRegionNumFileName, region);

        if (string.IsNullOrEmpty(dno))
        {
            return null;
        }
        return int.Parse(dno);
    }

    private static string FindMatchFetchSecondColumn(string fileName, string str)
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