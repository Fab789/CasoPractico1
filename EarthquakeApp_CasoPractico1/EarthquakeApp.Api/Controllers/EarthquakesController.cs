using System.Text.RegularExpressions;
using EarthquakeApp.Api.Services;
using EarthquakeApp.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EarthquakeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EarthquakesController : ControllerBase
{
    private static readonly Regex ValidName = new(@"^[\p{L} ]+$", RegexOptions.Compiled);
    private readonly IEarthquakeSearchService _search;
    private readonly IEarthquakeSyncService _sync;

    public EarthquakesController(IEarthquakeSearchService search, IEarthquakeSyncService sync)
    {
        _search = search;
        _sync = sync;
    }

    /// <summary>Busca terremotos por nombre de país (incluye información del país).</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(EarthquakeSearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Search([FromQuery] string? country, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(country))
            return BadRequest(new ErrorResponseDto { Status = 400, Message = "Debe indicar el nombre de un país." });
        if (country.Length > 100)
            return BadRequest(new ErrorResponseDto { Status = 400, Message = "El nombre no puede superar los 100 caracteres." });
        if (!ValidName.IsMatch(country))
            return BadRequest(new ErrorResponseDto { Status = 400, Message = "Solo se permiten letras y espacios (sin números ni caracteres especiales)." });

        var result = await _search.SearchByCountryAsync(country, ct);
        return Ok(result);
    }

    /// <summary>Solicita únicamente los registros nuevos a USGS y los agrega a la BD.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var inserted = await _sync.SyncAsync(onlyNew: true, ct);
        return Ok(new { newRecords = inserted });
    }
}
