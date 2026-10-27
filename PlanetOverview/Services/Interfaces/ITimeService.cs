using PlanetOverview.Models;

namespace PlanetOverview.Services.Interfaces;

public interface ITimeService
{
    Task<TimeOverview?> GetByCoordinatesAsync(double latitude, double longitude);
    Task<TimeOverview?> GetCurrentTimeAsync(string timeZone);
}