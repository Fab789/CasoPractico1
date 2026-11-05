using EarthquakeApp.Api.Exceptions;
using EarthquakeApp.Dto;

namespace EarthquakeApp.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (CountryNotFoundException ex)
        {
            await Write(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ExternalApiException ex)
        {
            _logger.LogWarning(ex, "Fallo en API externa");
            await Write(context, StatusCodes.Status502BadGateway, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            await Write(context, StatusCodes.Status500InternalServerError,
                "Ocurrió un error inesperado en el servidor.");
        }
    }

    private static async Task Write(HttpContext context, int status, string message)
    {
        if (context.Response.HasStarted) return;
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ErrorResponseDto { Status = status, Message = message });
    }
}
