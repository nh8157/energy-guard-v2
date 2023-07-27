using System.Reflection;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;

namespace EnergyPerformance.Services;

public class EnergyRateService: IEnergyRateService
{
    private static readonly string countryCodesFileName = "country_codes";
    private static readonly string dnoRegionNumFileName = "dno_region_numbers";
    private static readonly string eurostatYear = "2022";
    private static readonly string voltage = "HV";

    public async Task<double> GetEnergyRate(string countryName, string ukRegion="")
    {
        try
        {
            HttpClient client = new HttpClient();
            if (countryName.ToLower().Equals("united kingdom"))
            {
                if (string.IsNullOrEmpty(ukRegion))
                {
                    throw new ArgumentException("DNO must be provided for United Kingdom energy rate.");
                }
                int dno = GetDNO(ukRegion) ??
                    throw new ArgumentException("Please provide a valid UK Region.");

                var energyRateUK = await GetEnergyRateUK(client, dno);
                return energyRateUK;
            }
            var countryCode = GetCountryCode(countryName) ??
                throw new ArgumentException($"The country {countryName} is not supported.");

            var energyRateEurope = await GetEnergyRateEurope(client, countryCode);
            return energyRateEurope;
        } catch (Exception ex)
        {
            App.GetService<DebugModel>().AddMessage(ex.ToString());
            return 0;
        }
    }

    private static async Task<double> GetEnergyRateUK(HttpClient client, int dno)
    {
        string dateNow = DateTime.Now.ToString("dd-MM-yyyy");
        var url = $"https://odegdcpnma.execute-api.eu-west-2.amazonaws.com/development/prices?dno={dno}&voltage={voltage}&start={dateNow}&end={dateNow}";

        Uri energyCostsUri = new(url);
        var energyCostsApi = await ApiProcessor<EnergyCostsModel>.Load(client, energyCostsUri) ??
            throw new Exception("EnergyCosts API is not available.");

        return energyCostsApi.GetEnergyRateUK() / 100;
    }

    private static async Task<double> GetEnergyRateEurope(HttpClient client, string countryCode)
    {
        var url = $"https://ec.europa.eu/eurostat/api/dissemination/statistics/1.0/data/TEN00117/?format=JSON&time={eurostatYear}";

        Uri eurostatUri = new(url);
        var eurostatApi = await ApiProcessor<EurostatModel>.Load(client, eurostatUri) ??
            throw new Exception("Eurostat API is not available.");

        return eurostatApi.GetEnergyRate(countryCode);
    }

    private static string? GetCountryCode(string country)
    {
        var countryCode = FindMatchFetchSecondColumn(countryCodesFileName, country);

        if (string.IsNullOrEmpty(countryCode))
        {
            return null;
        }
        return countryCode.ToUpper();
    }

    private static int? GetDNO(string region)
    {
        var dno = FindMatchFetchSecondColumn(dnoRegionNumFileName, region);

        if (string.IsNullOrEmpty(dno))
        {
            return null;
        }
        return Int32.Parse(dno);
    }

    private static string FindMatchFetchSecondColumn(string fileName, string str)
    {
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var filePath = Path.Combine(basePath, fileName);

        var matchEqvalent = "";
        using var read = new StreamReader(filePath);

        while (!read.EndOfStream)
        {
            var line = read.ReadLine();
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