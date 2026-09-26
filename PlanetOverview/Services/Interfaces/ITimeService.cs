using PlanetOverview.Models;

namespace PlanetOverview.Services.Interfaces;

public interface ITimeService
{
    Task<TimeOverview?> GetCurrentTimeAsync(string timeZone);
}