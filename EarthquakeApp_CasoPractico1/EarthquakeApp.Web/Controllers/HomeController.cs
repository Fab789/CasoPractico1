using EarthquakeApp.Web.Models;
using EarthquakeApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EarthquakeApp.Web.Controllers;

public class HomeController : Controller
{
    private readonly IEarthquakeApiClient _api;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IEarthquakeApiClient api, ILogger<HomeController> logger)
    {
        _api = api;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index() => View(new SearchViewModel());

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchViewModel model, CancellationToken ct)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
                return StatusCode(400, new { title = "Búsqueda no válida", message });
            }

            var result = await _api.SearchAsync(model.Country.Trim(), ct);
            return PartialView("_Results", result);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex, "El API respondió con error {Status}", ex.StatusCode);
            return ex.StatusCode switch
            {
                404 => StatusCode(404, new { title = "País no encontrado", message = ex.Message }),
                400 => StatusCode(400, new { title = "Búsqueda no válida", message = ex.Message }),
                _ => StatusCode(503, new { title = "Servicio no disponible", message = ex.Message })
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "No se pudo conectar con el API");
            return StatusCode(503, new { title = "Servicio no disponible", message = "No pudimos conectarnos con el servicio de terremotos. Intenta de nuevo en unos minutos." });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Tiempo de espera agotado");
            return StatusCode(504, new { title = "Tiempo agotado", message = "La consulta tardó demasiado. Intenta de nuevo." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en la búsqueda");
            return StatusCode(503, new { title = "Algo salió mal", message = "Ocurrió un error inesperado. Intenta de nuevo más tarde." });
        }
    }

    [HttpGet]
    public IActionResult Error() => View();
}
