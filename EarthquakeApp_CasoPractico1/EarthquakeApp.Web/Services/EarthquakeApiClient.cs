using EarthquakeApp.Dto;
using Microsoft.Extensions.Caching.Memory;

namespace EarthquakeApp.Web.Services;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public ApiException(int statusCode, string message) : base(message) => StatusCode = statusCode;
}

public interface IEarthquakeApiClient
{
    Task<EarthquakeSearchResultDto> SearchAsync(string country, CancellationToken ct = default);
}

/// <summary>Servicio del MVC que llama al API creado (con memory cache).
/// Las excepciones se manejan con try/catch únicamente en el controller.</summary>
public class EarthquakeApiClient : IEarthquakeApiClient
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly int _cacheMinutes;

    public EarthquakeApiClient(HttpClient http, IMemoryCache cache, IConfiguration config)
    {
        _http = http;
        _cache = cache;
        _cacheMinutes = config.GetValue("EarthquakeApi:CacheMinutes", 5);
    }

    public async Task<EarthquakeSearchResultDto> SearchAsync(string country, CancellationToken ct = default)
    {
        var key = $"web-search:{country.Trim().ToLowerInvariant()}";
        if (_cache.TryGetValue(key, out EarthquakeSearchResultDto? cached) && cached is not null)
            return cached;

        using var response = await _http.GetAsync($"api/earthquakes/search?country={Uri.EscapeDataString(country)}", ct);

        if (!response.IsSuccessStatusCode)
        {
            ErrorResponseDto? error = null;
            if (response.Content.Headers.ContentType?.MediaType?.Contains("json") == true)
                error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(cancellationToken: ct);

            throw new ApiException((int)response.StatusCode,
                error?.Message ?? "El servicio devolvió una respuesta inesperada.");
        }

        var result = await response.Content.ReadFromJsonAsync<EarthquakeSearchResultDto>(cancellationToken: ct)
                     ?? throw new ApiException(502, "El servicio devolvió una respuesta vacía.");

        _cache.Set(key, result, TimeSpan.FromMinutes(_cacheMinutes));
        return result;
    }
}
