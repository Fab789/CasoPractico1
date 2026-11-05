using System.Globalization;
using EarthquakeApp.Api.Models;
using EarthquakeApp.Data.Repositories;
using Microsoft.Extensions.Options;

namespace EarthquakeApp.Api.Services;

public interface IEarthquakeSyncService
{
    /// <param name="onlyNew">false: carga inicial (solo si la BD está vacía). true: trae únicamente registros nuevos.</param>
    Task<int> SyncAsync(bool onlyNew, CancellationToken ct = default);
}

public class EarthquakeSyncService : IEarthquakeSyncService
{
    private readonly IUsgsService _usgs;
    private readonly IEarthquakeRepository _repository;
    private readonly UsgsOptions _options;
    private readonly ILogger<EarthquakeSyncService> _logger;

    public EarthquakeSyncService(IUsgsService usgs, IEarthquakeRepository repository,
        IOptions<UsgsOptions> options, ILogger<EarthquakeSyncService> logger)
    {
        _usgs = usgs;
        _repository = repository;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> SyncAsync(bool onlyNew, CancellationToken ct = default)
    {
        var defaultStart = DateTime.Parse(_options.StartTime, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        DateTime start;

        if (onlyNew)
        {
            // Solo registros más recientes que el último guardado
            start = await _repository.GetLatestTimeAsync(ct) ?? defaultStart;
        }
        else
        {
            if (await _repository.AnyAsync(ct))
            {
                _logger.LogInformation("La BD ya tiene datos: no se llama al API de USGS.");
                return 0;
            }
            start = defaultStart;
        }

        _logger.LogInformation("Consultando USGS desde {Start}...", start);
        var earthquakes = await _usgs.GetEarthquakesAsync(start, ct);
        var inserted = await _repository.AddRangeAsync(earthquakes, ct);
        _logger.LogInformation("Registros nuevos guardados: {Count}", inserted);
        return inserted;
    }
}
