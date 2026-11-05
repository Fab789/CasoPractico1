using EarthquakeApp.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EarthquakeApp.Data.Repositories;

public class EarthquakeRepository : IEarthquakeRepository
{
    private const int BatchSize = 1000;
    private readonly EarthquakeDbContext _context;

    public EarthquakeRepository(EarthquakeDbContext context) => _context = context;

    public Task<bool> AnyAsync(CancellationToken ct = default) =>
        _context.Earthquakes.AnyAsync(ct);

    public async Task<DateTime?> GetLatestTimeAsync(CancellationToken ct = default)
    {
        if (!await _context.Earthquakes.AnyAsync(ct)) return null;
        return await _context.Earthquakes.MaxAsync(e => e.TimeUtc, ct);
    }

    public async Task<int> AddRangeAsync(IEnumerable<Earthquake> earthquakes, CancellationToken ct = default)
    {
        var unique = earthquakes.DistinctBy(e => e.UsgsId).ToList();
        var inserted = 0;

        foreach (var chunk in unique.Chunk(BatchSize))
        {
            var ids = chunk.Select(e => e.UsgsId).ToList();
            var existing = await _context.Earthquakes
                .Where(e => ids.Contains(e.UsgsId))
                .Select(e => e.UsgsId)
                .ToListAsync(ct);

            var toInsert = chunk.Where(e => !existing.Contains(e.UsgsId)).ToList();
            if (toInsert.Count == 0) continue;

            await _context.Earthquakes.AddRangeAsync(toInsert, ct);
            inserted += await _context.SaveChangesAsync(ct);
            _context.ChangeTracker.Clear();
        }

        return inserted;
    }

    public async Task<IReadOnlyList<Earthquake>> GetByCountryAsync(IReadOnlyCollection<string> countryNames, int max, CancellationToken ct = default)
    {
        var names = countryNames.ToList();
        return await _context.Earthquakes
            .AsNoTracking()
            .Where(e => names.Contains(e.Country))
            .OrderByDescending(e => e.TimeUtc)
            .Take(max)
            .ToListAsync(ct);
    }

    public async Task<CountryStats> GetStatsByCountryAsync(IReadOnlyCollection<string> countryNames, CancellationToken ct = default)
    {
        var names = countryNames.ToList();
        var query = _context.Earthquakes.AsNoTracking().Where(e => names.Contains(e.Country));

        var total = await query.CountAsync(ct);
        if (total == 0) return new CountryStats(0, null, null);

        var max = await query.MaxAsync(e => e.Magnitude, ct);
        var avg = await query.AverageAsync(e => e.Magnitude, ct);
        return new CountryStats(total, max, avg);
    }
}

