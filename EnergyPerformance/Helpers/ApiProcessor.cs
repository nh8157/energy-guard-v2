namespace EnergyPerformance.Helpers;


public class ApiProcessor<T>
{
    public static async Task<T?> Load(HttpClient client, Uri uri)
    {
        await using Stream stream = await client.GetStreamAsync(uri);
        var response = await JsonSerializer.DeserializeAsync<T>(stream);

        return response;
    }
}
