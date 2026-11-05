using EarthquakeApp.Api.Exceptions;
using EarthquakeApp.Api.Helpers;
using EarthquakeApp.Api.Models;
using EarthquakeApp.Data.Repositories;
using EarthquakeApp.Dto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace EarthquakeApp.Api.Services;

public interface IEarthquakeSearchService
{
    Task<EarthquakeSearchResultDto> SearchByCountryAsync(string country, CancellationToken ct = default);
}

/// <summary>Une la info del país (restcountries) con los terremotos de la BD en un solo objeto.</summary>
public class EarthquakeSearchService : IEarthquakeSearchService
{
    private const int MaxResults = 100;
    private readonly IEarthquakeRepository _repository;
    private readonly IRestCountriesService _countries;
    private readonly IMemoryCache _cache;
    private readonly UsgsOptions _options;

    public EarthquakeSearchService(IEarthquakeRepository repository, IRestCountriesService countries,
        IMemoryCache cache, IOptions<UsgsOptions> options)
    {
        _repository = repository;
        _countries = countries;
        _cache = cache;
        _options = options.Value;
    }

    public async Task<EarthquakeSearchResultDto> SearchByCountryAsync(string country, CancellationToken ct = default)
    {
        var name = country.Trim();
        var key = $"search:{name.ToLowerInvariant()}";

        if (_cache.TryGetValue(key, out EarthquakeSearchResultDto? cached) && cached is not null)
            return cached;

        var countryInfo = await _countries.GetByNameAsync(name, ct)
                          ?? throw new CountryNotFoundException(name);

        var names = new[] { countryInfo.CommonName, countryInfo.OfficialName, name }
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var items = await _repository.GetByCountryAsync(names, MaxResults, ct);
        var stats = await _repository.GetStatsByCountryAsync(names, ct);

        var result = new EarthquakeSearchResultDto
        {
            SearchedCountry = name,
            Country = countryInfo,
            TotalEarthquakes = stats.Total,
            MaxMagnitude = stats.MaxMagnitude,
            AverageMagnitude = stats.AverageMagnitude is null ? null : Math.Round(stats.AverageMagnitude.Value, 2),
            Earthquakes = items.Select(e => e.ToDto()).ToList()
        };

        _cache.Set(key, result, TimeSpan.FromMinutes(_options.CacheMinutes));
        return result;
    }
}
