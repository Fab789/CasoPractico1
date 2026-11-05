using System.Globalization;
using System.Net;
using System.Text.Json;
using EarthquakeApp.Api.Exceptions;
using EarthquakeApp.Api.Models;
using EarthquakeApp.Dto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace EarthquakeApp.Api.Services;

public interface IRestCountriesService
{
    Task<CountryDto?> GetByNameAsync(string name, CancellationToken ct = default);
}

/// <summary>Servicio que llama a REST Countries v5 (con API Key y memory cache).</summary>
public class RestCountriesService : IRestCountriesService
{
    private const string BaseFields =
        "names.common,names.official,codes.alpha_2,capitals,region,subregion,population,area.kilometers,flag.url_png,flag.url_svg,languages,currencies,timezones";

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly RestCountriesOptions _options;

    public RestCountriesService(HttpClient http, IMemoryCache cache, IOptions<RestCountriesOptions> options)
    {
        _http = http;
        _cache = cache;
        _options = options.Value;
    }

    public Task<CountryDto?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var key = $"country:{name.Trim().ToLowerInvariant()}";
        return _cache.GetOrCreateAsync<CountryDto?>(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheMinutes);
            return await FetchAsync(name.Trim(), ct);
        });
    }

    private async Task<CountryDto?> FetchAsync(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || _options.ApiKey.StartsWith("REEMPLAZAR"))
            throw new ExternalApiException("Falta configurar la API Key de REST Countries en el appsettings.json del API.");

        var escaped = Uri.EscapeDataString(name);

        // 1) Por nombre (common, official, alternos, nativos)
        var list = await QueryAsync($"name?q={escaped}&limit=25&response_fields={BaseFields}", ct);
        var match = PickByName(list, name);
        if (match is not null) return Map(match);

        // 2) Por traducción (ej. "Japón", "Alemania")
        list = await QueryAsync($"names.translations?q={escaped}&limit=25&response_fields={BaseFields},names.translations", ct);
        match = list.FirstOrDefault(c => TranslationMatches(c, name)) ?? (list.Count == 1 ? list[0] : null);
        return match is null ? null : Map(match);
    }

    private async Task<List<RcCountry>> QueryAsync(string relativeUrl, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_options.ApiKey}");

            using var response = await _http.SendAsync(request, ct);

            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
                return new();

            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalApiException(response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "La API Key de REST Countries no es válida o expiró. Revisa el appsettings.json del API.",
                    HttpStatusCode.Forbidden => "La cuenta de REST Countries está bloqueada o excedió su límite mensual.",
                    HttpStatusCode.TooManyRequests => "Demasiadas consultas a REST Countries. Intenta de nuevo en unos segundos.",
                    HttpStatusCode.Gone => "La versión del API de REST Countries configurada ya no está activa.",
                    _ => "El servicio de información de países no está disponible en este momento."
                });
            }

            var body = await response.Content.ReadFromJsonAsync<RcResponse>(cancellationToken: ct);
            return body?.Data?.Objects ?? new();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            throw new ExternalApiException("No fue posible consultar el servicio de información de países.", ex);
        }
    }

    private static RcCountry? PickByName(List<RcCountry> list, string name) =>
        list.FirstOrDefault(c => Eq(c.Names.Common, name) || Eq(c.Names.Official, name))
        ?? (list.Count == 1 ? list[0] : null);

    private static bool Eq(string? a, string? b) =>
        a is not null && b is not null &&
        string.Compare(a, b, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;

    private static bool TranslationMatches(RcCountry c, string name)
    {
        if (c.Names.Translations is not { ValueKind: JsonValueKind.Object } t) return false;
        foreach (var lang in t.EnumerateObject())
        {
            if (lang.Value.ValueKind != JsonValueKind.Object) continue;
            foreach (var prop in new[] { "common", "official" })
                if (lang.Value.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String && Eq(v.GetString(), name))
                    return true;
        }
        return false;
    }

    private static CountryDto Map(RcCountry c) => new()
    {
        CommonName = c.Names.Common,
        OfficialName = c.Names.Official,
        Cca2 = c.Codes?.Alpha2 ?? string.Empty,
        Capital = string.Join(", ", (c.Capitals ?? new()).Select(x => x.Name).Where(x => !string.IsNullOrWhiteSpace(x))),
        Region = c.Region ?? string.Empty,
        Subregion = c.Subregion ?? string.Empty,
        Population = (long)(c.Population ?? 0),
        Area = c.Area?.Kilometers ?? 0,
        FlagUrl = c.Flag?.UrlPng ?? c.Flag?.UrlSvg ?? string.Empty,
        Languages = ExtractNames(c.Languages, includeSymbol: false),
        Currencies = ExtractNames(c.Currencies, includeSymbol: true),
        Timezones = c.Timezones ?? new()
    };

    /// <summary>Lectura tolerante de languages/currencies (objeto o arreglo).</summary>
    private static List<string> ExtractNames(JsonElement? element, bool includeSymbol)
    {
        var result = new List<string>();
        if (element is not { } el) return result;

        var items = new List<(string? Key, JsonElement Value)>();
        if (el.ValueKind == JsonValueKind.Array)
            items.AddRange(el.EnumerateArray().Select(v => ((string?)null, v)));
        else if (el.ValueKind == JsonValueKind.Object)
            items.AddRange(el.EnumerateObject().Select(p => ((string?)p.Name, p.Value)));

        foreach (var (key, value) in items)
        {
            string? label = value.ValueKind == JsonValueKind.String ? value.GetString()
                          : value.ValueKind == JsonValueKind.Object ? FindName(value) : null;
            label ??= key;
            if (string.IsNullOrWhiteSpace(label)) continue;

            if (includeSymbol && value.ValueKind == JsonValueKind.Object &&
                value.TryGetProperty("symbol", out var sym) && sym.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(sym.GetString()))
                label = $"{label} ({sym.GetString()})";

            result.Add(label);
        }
        return result.Distinct().ToList();
    }

    private static string? FindName(JsonElement obj)
    {
        string? fallback = null;
        foreach (var p in obj.EnumerateObject())
        {
            var k = p.Name.ToLowerInvariant();
            if (p.Value.ValueKind == JsonValueKind.String)
            {
                if (k == "name" || k.Contains("english")) return p.Value.GetString();
                if (fallback is null && k.Contains("name")) fallback = p.Value.GetString();
            }
            else if (p.Value.ValueKind == JsonValueKind.Object)
            {
                fallback ??= FindName(p.Value);
            }
        }
        return fallback;
    }
}
