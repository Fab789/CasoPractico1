namespace EarthquakeApp.Api.Helpers;

/// <summary>
/// USGS no trae un campo "país"; viene dentro de "place"
/// (ej. "10 km SW of Iquique, Chile"). Se toma lo que está después de la última coma.
/// </summary>
public static class CountryExtractor
{
    private static readonly HashSet<string> UsStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "Alabama","Alaska","Arizona","Arkansas","California","Colorado","Connecticut","Delaware","Florida",
        "Georgia","Hawaii","Idaho","Illinois","Indiana","Iowa","Kansas","Kentucky","Louisiana","Maine",
        "Maryland","Massachusetts","Michigan","Minnesota","Mississippi","Missouri","Montana","Nebraska",
        "Nevada","New Hampshire","New Jersey","New Mexico","New York","North Carolina","North Dakota",
        "Ohio","Oklahoma","Oregon","Pennsylvania","Rhode Island","South Carolina","South Dakota",
        "Tennessee","Texas","Utah","Vermont","Virginia","Washington","West Virginia","Wisconsin","Wyoming"
    };

    public static string FromPlace(string? place)
    {
        if (string.IsNullOrWhiteSpace(place)) return "Unknown";

        var idx = place.LastIndexOf(',');
        var part = (idx >= 0 ? place[(idx + 1)..] : place).Trim();

        if (part.EndsWith(" region", StringComparison.OrdinalIgnoreCase))
            part = part[..^7].Trim();

        if (part.Length == 2 && part.All(char.IsUpper)) return "United States";
        if (UsStates.Contains(part)) return "United States";

        return part.Length > 150 ? part[..150] : part;
    }
}
