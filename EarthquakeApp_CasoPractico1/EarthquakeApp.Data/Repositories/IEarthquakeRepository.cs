using EarthquakeApp.Data.Entities;

namespace EarthquakeApp.Data.Repositories;

public record CountryStats(int Total, double? MaxMagnitude, double? AverageMagnitude);

public interface IEarthquakeRepository
{
    Task<bool> AnyAsync(CancellationToken ct = default);
    Task<DateTime?> GetLatestTimeAsync(CancellationToken ct = default);
    Task<int> AddRangeAsync(IEnumerable<Earthquake> earthquakes, CancellationToken ct = default);
    Task<IReadOnlyList<Earthquake>> GetByCountryAsync(IReadOnlyCollection<string> countryNames, int max, CancellationToken ct = default);
    Task<CountryStats> GetStatsByCountryAsync(IReadOnlyCollection<string> countryNames, CancellationToken ct = default);
}
