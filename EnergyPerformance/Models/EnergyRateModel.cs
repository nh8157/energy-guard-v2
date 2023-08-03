using System.Text.Json.Serialization;

namespace EnergyPerformance.Models;


public record class EnergyCostsModel(
    [property: JsonPropertyName("data")] Region Data)
{
    public double GetEnergyRateUK()
    {
        return Data.Data[0].Overall;
    }
}

public record class Region(
    [property: JsonPropertyName("data")] List<Rate> Data);

public record class Rate(
    [property: JsonPropertyName("Overall")] double Overall);


public record class EurostatModel(
    [property: JsonPropertyName("value")] Dictionary<string, double> Value,
    [property: JsonPropertyName("dimension")] Geographies Dimension)
{
    private Dictionary<string, double> GetEnergyRates()
    {
        Dictionary<string, string> countryCodes = Dimension.Geo.Category.CountryCodes();
        Dictionary<string, double> energyRates = new();

        foreach (KeyValuePair<string, double> val in Value)
        {
            var index = val.Key;
            var rate = val.Value;

            if (countryCodes.ContainsKey(index))
            {
                energyRates.Add(countryCodes[index].ToUpper(), rate);
            }
        }
        return energyRates;
    }

    public double GetEnergyRate(string countryCode)
    {
        Dictionary<string, double> energyRates = GetEnergyRates();
        return energyRates[countryCode];
    }
}

public record class Geographies(
    [property: JsonPropertyName("geo")] Categories Geo);

public record class Categories(
    [property: JsonPropertyName("category")] Countries Category);

public record class Countries(
    [property: JsonPropertyName("index")] Dictionary<string, int> Index,
    [property: JsonPropertyName("label")] Dictionary<string, string> Label)
{
    public Dictionary<string, string> CountryCodes()
    {
        return Index.ToDictionary(x => x.Value.ToString(), x => x.Key); ;
    }
}

public record class EnergyNetworksModel(
    [property: JsonPropertyName("Postcode")] string Postcode,
    [property: JsonPropertyName("DNOCode")] string DnoCode);
