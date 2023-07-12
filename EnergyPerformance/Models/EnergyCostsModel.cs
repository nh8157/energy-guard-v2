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