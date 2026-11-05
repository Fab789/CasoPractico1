using System.Globalization;
using EarthquakeApp.Api.Exceptions;
using EarthquakeApp.Api.Helpers;
using EarthquakeApp.Api.Models;
using EarthquakeApp.Data.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace EarthquakeApp.Api.Services;

public interface IUsgsService
{
    Task<List<Earthquake>> GetEarthquakesAsync(DateTime startUtc, CancellationToken ct = default);
}

/// <summary>Servicio que llama al API de USGS (con memory cache).</summary>
public class UsgsService : IUsgsService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly UsgsOptions _options;

    public UsgsService(HttpClient http, IMemoryCache cache, IOptions<UsgsOptions> options)
    {
        _http = http;
        _cache = cache;
        _options = options.Value;
    }

    public async Task<List<Earthquake>> GetEarthquakesAsync(DateTime startUtc, CancellationToken ct = default)
    {
        var start = startUtc.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
        var minMag = _options.MinMagnitude.ToString(CultureInfo.InvariantCulture);
        var url = $"{_options.BaseUrl}?format=geojson&starttime={start}&minmagnitude={minMag}&orderby=time-asc";

        var response = await _cache.GetOrCreateAsync($"usgs:{url}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheMinutes);
            try
            {
                return await _http.GetFromJsonAsync<UsgsResponse>(url, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                throw new ExternalApiException("No fue posible consultar el servicio de terremotos (USGS).", ex);
            }
        });

        return (response?.Features ?? new()).Select(ToEntity).Where(e => e is not null).Select(e => e!).ToList();
    }

    private static Earthquake? ToEntity(UsgsFeature f)
    {
        var coords = f.Geometry?.Coordinates;
        if (string.IsNullOrWhiteSpace(f.Id) || coords is null || coords.Count < 2) return null;

        var place = string.IsNullOrWhiteSpace(f.Properties.Place) ? "Unknown" : f.Properties.Place!;
        return new Earthquake
        {
            UsgsId = f.Id.Length > 50 ? f.Id[..50] : f.Id,
            Magnitude = f.Properties.Mag,
            Place = place.Length > 500 ? place[..500] : place,
            Country = CountryExtractor.FromPlace(place),
            TimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(f.Properties.Time).UtcDateTime,
            Longitude = coords[0],
            Latitude = coords[1],
            DepthKm = coords.Count > 2 ? coords[2] : null,
            Url = f.Properties.Url,
            Title = f.Properties.Title,
            Type = f.Properties.Type
        };
    }
}
