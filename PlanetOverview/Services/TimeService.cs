using PlanetOverview.Models;
using PlanetOverview.Models.ApiResponses;
using PlanetOverview.Services.Interfaces;

namespace PlanetOverview.Services;

public class TimeService : ITimeService
{
    private readonly HttpClient _httpClient;

    public TimeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TimeOverview?> GetByCoordinatesAsync(double latitude, double longitude)
    {
        var url = FormattableString.Invariant($"https://timeapi.io/api/Time/current/coordinate?latitude={latitude}&longitude={longitude}");
        var response = await ApiClient.ReadAsync<TimeApiResponse>(_httpClient, url);
        if (response == null || !DateTime.TryParse(response.DateTime, out var local)) return null;
        return new TimeOverview { TimeZone = response.TimeZone, LocalDateTime = local,
            Date = local.ToString("dd/MM/yyyy"), Time = local.ToString("HH:mm:ss") };
    }

    public async Task<TimeOverview?> GetCurrentTimeAsync(string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
        {
            return null;
        }

        var encodedTimeZone = Uri.EscapeDataString(timeZone.Trim());

        var apiResponse = await ApiClient.ReadAsync<TimeApiResponse>(_httpClient,
            $"https://timeapi.io/api/Time/current/zone?timeZone={encodedTimeZone}");

        if (apiResponse == null ||
            string.IsNullOrWhiteSpace(apiResponse.DateTime))
        {
            return null;
        }

        if (!DateTime.TryParse(
                apiResponse.DateTime,
                out var localDateTime))
        {
            return null;
        }

        return new TimeOverview
        {
            TimeZone = apiResponse.TimeZone ?? timeZone,
            LocalDateTime = localDateTime,
            Date = localDateTime.ToString("dd/MM/yyyy"),
            Time = localDateTime.ToString("HH:mm:ss")
        };
    }
}