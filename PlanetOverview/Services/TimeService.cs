using System.Net;
using System.Net.Http.Json;
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

    public async Task<TimeOverview?> GetCurrentTimeAsync(string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
        {
            return null;
        }

        var encodedTimeZone = Uri.EscapeDataString(timeZone.Trim());

        var response = await _httpClient.GetAsync(
            $"https://timeapi.io/api/Time/current/zone?timeZone={encodedTimeZone}"
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var apiResponse = await response.Content
            .ReadFromJsonAsync<TimeApiResponse>();

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