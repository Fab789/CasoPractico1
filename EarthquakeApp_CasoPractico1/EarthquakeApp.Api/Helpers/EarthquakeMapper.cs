using EarthquakeApp.Data.Entities;
using EarthquakeApp.Dto;

namespace EarthquakeApp.Api.Helpers;

/// <summary>Convierte Entity (EF) a DTO: la entidad nunca sale del API.</summary>
public static class EarthquakeMapper
{
    public static EarthquakeDto ToDto(this Earthquake e) => new()
    {
        Id = e.Id,
        UsgsId = e.UsgsId,
        Magnitude = e.Magnitude,
        Place = e.Place,
        Country = e.Country,
        TimeUtc = DateTime.SpecifyKind(e.TimeUtc, DateTimeKind.Utc),
        Latitude = e.Latitude,
        Longitude = e.Longitude,
        DepthKm = e.DepthKm,
        Url = e.Url,
        Title = e.Title,
        Type = e.Type
    };
}
