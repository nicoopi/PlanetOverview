using System;
using System.Net.Http;
using System.Threading.Tasks;
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

    public async Task<(bool Sucesso, TimeOverview? Dados, string? MensagemErro)> GetByCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            var url = FormattableString.Invariant($"https://timeapi.io/api/Time/current/coordinate?latitude={latitude}&longitude={longitude}");
            var response = await ApiClient.ReadAsync<TimeApiResponse>(_httpClient, url);
            
            if (response == null || !DateTime.TryParse(response.DateTime, out var local)) 
                return (false, null, "Não foi possível encontrar o horário para estas coordenadas.");

            var overview = new TimeOverview 
            { 
                TimeZone = response.TimeZone, 
                LocalDateTime = local,
                Date = local.ToString("dd/MM/yyyy"), 
                Time = local.ToString("HH:mm:ss") 
            };
            
            return (true, overview, null);
        }
        catch (HttpRequestException)
        {
            return (false, null, "Serviço de horário temporariamente indisponível. Verifique a conexão.");
        }
        catch (Exception)
        {
            return (false, null, "Ocorreu um erro inesperado ao buscar o horário local.");
        }
    }

    public async Task<(bool Sucesso, TimeOverview? Dados, string? MensagemErro)> GetCurrentTimeAsync(string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
        {
            return (false, null, "Fuso horário não informado.");
        }

        try
        {
            var encodedTimeZone = Uri.EscapeDataString(timeZone.Trim());
            var apiResponse = await ApiClient.ReadAsync<TimeApiResponse>(_httpClient,
                $"https://timeapi.io/api/Time/current/zone?timeZone={encodedTimeZone}");

            if (apiResponse == null || string.IsNullOrWhiteSpace(apiResponse.DateTime))
            {
                return (false, null, $"Não foi possível encontrar o fuso horário '{timeZone}'.");
            }

            if (!DateTime.TryParse(apiResponse.DateTime, out var localDateTime))
            {
                return (false, null, "Erro ao processar o formato de data/hora retornado.");
            }

            var overview = new TimeOverview
            {
                TimeZone = apiResponse.TimeZone ?? timeZone,
                LocalDateTime = localDateTime,
                Date = localDateTime.ToString("dd/MM/yyyy"),
                Time = localDateTime.ToString("HH:mm:ss")
            };

            return (true, overview, null);
        }
        catch (HttpRequestException)
        {
            return (false, null, "Serviço de fuso horário temporariamente indisponível. Verifique a conexão.");
        }
        catch (Exception)
        {
            return (false, null, "Ocorreu um erro interno ao buscar as informações de fuso horário.");
        }
    }
}