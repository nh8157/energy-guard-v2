using System.Text.Json;

namespace EnergyPerformance.Helpers;


public class ApiProcessor<T>
{
    public static async Task<T?> Load(HttpClient client, string uri)
    {
        await using var stream = await client.GetStreamAsync(uri);
        var response = await JsonSerializer.DeserializeAsync<T>(stream);

        return response;
    }
}
