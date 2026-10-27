using System.Text.Json;
namespace PlanetOverview.Services;
internal static class ApiClient
{
    internal static async Task<T?> ReadAsync<T>(HttpClient client, string url) where T : class
    {
        try
        {
            using var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<T>() : null;
        }
        catch (Exception error) when (error is HttpRequestException or OperationCanceledException or JsonException or NotSupportedException)
        {
            return null;
        }
    }
}
