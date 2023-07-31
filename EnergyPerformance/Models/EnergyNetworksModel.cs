using System.Text.Json.Serialization;

namespace EnergyPerformance.Models;


public record class EnergyNetworksModel(
    [property: JsonPropertyName("Postcode")] string Postcode,
    [property: JsonPropertyName("DNOCode")] string DnoCode);
