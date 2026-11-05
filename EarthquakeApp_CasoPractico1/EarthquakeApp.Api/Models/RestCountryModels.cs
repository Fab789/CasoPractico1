using System.Text.Json;
using System.Text.Json.Serialization;

namespace EarthquakeApp.Api.Models;

// Modelos para deserializar REST Countries v5:  { "data": { "objects": [ ... ], "meta": { ... } } }
public class RcResponse
{
    public RcData? Data { get; set; }
}

public class RcData
{
    public List<RcCountry> Objects { get; set; } = new();
}

public class RcCountry
{
    public RcNames Names { get; set; } = new();
    public RcCodes? Codes { get; set; }
    public List<RcCapital>? Capitals { get; set; }
    public string? Region { get; set; }
    public string? Subregion { get; set; }
    public double? Population { get; set; }
    public RcArea? Area { get; set; }
    public RcFlag? Flag { get; set; }
    public List<string>? Timezones { get; set; }
    // Estructuras anidadas variables: se leen de forma tolerante
    public JsonElement? Languages { get; set; }
    public JsonElement? Currencies { get; set; }
}

public class RcNames
{
    public string Common { get; set; } = string.Empty;
    public string Official { get; set; } = string.Empty;
    public JsonElement? Translations { get; set; }
}

public class RcCodes
{
    [JsonPropertyName("alpha_2")] public string? Alpha2 { get; set; }
}

public class RcCapital
{
    public string? Name { get; set; }
}

public class RcArea
{
    public double? Kilometers { get; set; }
}

public class RcFlag
{
    [JsonPropertyName("url_png")] public string? UrlPng { get; set; }
    [JsonPropertyName("url_svg")] public string? UrlSvg { get; set; }
}
