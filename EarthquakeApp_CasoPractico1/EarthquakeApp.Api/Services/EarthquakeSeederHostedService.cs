namespace EarthquakeApp.Api.Services;

/// <summary>Al iniciar la app: si la BD está vacía llama a USGS; si ya hay datos no llama.</summary>
public class EarthquakeSeederHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EarthquakeSeederHostedService> _logger;

    public EarthquakeSeederHostedService(IServiceScopeFactory scopeFactory, ILogger<EarthquakeSeederHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var sync = scope.ServiceProvider.GetRequiredService<IEarthquakeSyncService>();
            await sync.SyncAsync(onlyNew: false, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo completar la carga inicial de terremotos.");
        }
    }
}
