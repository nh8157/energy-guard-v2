using System.Diagnostics;
using System.Reflection;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using Microsoft.Extensions.Hosting;

namespace EnergyPerformance.Services;

public class EnergyRateService: BackgroundService
{
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromHours(1));
    private readonly EnergyRateInfo _energyRateInfo;
    private readonly LocationInfo _locationInfo;

    private const string _ukUrl = "https://odegdcpnma.execute-api.eu-west-2.amazonaws.com/development/prices?dno={0}&voltage={1}&start={2}&end={3}";
    private const string _euUrl = "https://ec.europa.eu/eurostat/api/dissemination/statistics/1.0/data/TEN00117/?format=JSON&time={0}";
    private const string _countryCodesFileName = "country_codes";
    private const string _dnoRegionNumFileName = "dno_region_numbers";

    private readonly HttpClient _httpClient;

    private readonly string _eurostatYear = "2022";
    private readonly string _voltage = "HV";

    public EnergyRateService(LocationInfo locationInfo, EnergyRateInfo energyRateInfo)
    {
        _locationInfo = locationInfo;
        _energyRateInfo = energyRateInfo;
        _httpClient = new HttpClient();
    }

    protected async override Task ExecuteAsync(CancellationToken token)
    {
        do
            await DoAsync();
        while (await _periodicTimer.WaitForNextTickAsync(token) && !token.IsCancellationRequested);
    }

    public async Task DoAsync()
    {
        var country = _locationInfo.Country.ToLower();
        // get a postcode that's all lower case and has no white space
        var postCode = _locationInfo.PostCode.Replace(" ", "");
        var countryCode = GetCountryCode(country);
        double rate = 0;

        if (country.ToLower().Equals("united kingdom"))
            // TODO: get DNO from postCode using remote API
            rate = await GetEnergyRateUKAsync(12);

        else if (countryCode is not null)
            // country is in europe
            rate = await GetEnergyRateEuropeAsync(countryCode);

        _energyRateInfo.EnergyRate = rate;
        Debug.WriteLine($"Fetching energy rate live for {country}: {rate}");
    }

    /// <summary>
    /// Retrieves the energy rate of the United Kingdom for a specific region, 
    /// requiring the corresponding DNO (Distribution Network Operator) number. 
    /// For further details about the DNO, please visit: 
    /// https://electricitycosts.org.uk/api/
    /// </summary>
    /// <param name="dno">
    /// Distribution Network Operator number.
    /// </param>
    private async Task<double> GetEnergyRateUKAsync(int dno)
    {
        var dateNow = DateTime.Now.ToString("dd-MM-yyyy");
        var url = String.Format(_ukUrl, dno, _voltage, dateNow, dateNow);

        // fetching and deserializing the data from remote API
        var energyCostsApi = await ApiProcessor<EnergyCostsModel>.Load(_httpClient, url) ??
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
    /// <param name="countryCode">
    /// Two-letter country code.
    /// </param>
    private async Task<double> GetEnergyRateEuropeAsync(string countryCode)
    {
        var url = String.Format(_euUrl, _eurostatYear);

        var eurostatApi = await ApiProcessor<EurostatModel>.Load(_httpClient, url) ??
        throw new Exception("Eurostat API is not available.");
        return eurostatApi.GetEnergyRate(countryCode);
    }

    /// <summary>
    /// Retrieves the country code of a given country within Europe.
    /// <summary>
    /// <param name="country">
    /// The name of the country.
    /// </param>
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
    /// <param name="ukRegion">
    /// A region in the UK (e.g. London).
    /// </param>
    private int? GetDNO(string ukRegion)
    {
        var dno = FindMatch(_dnoRegionNumFileName, ukRegion);

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
    /// <param name="fileName">
    /// The name of the file (country_code or dno_region_numbers).
    /// </param>
    /// <param name="str">
    /// The countries or UK regions to match.
    /// </param>
    private string FindMatch(string fileName, string str)
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