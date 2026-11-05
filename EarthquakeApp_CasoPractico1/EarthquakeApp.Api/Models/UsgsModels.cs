namespace EarthquakeApp.Api.Models;

// Modelos para deserializar el GeoJSON de USGS
public class UsgsResponse
{
    public List<UsgsFeature> Features { get; set; } = new();
}

public class UsgsFeature
{
    public string Id { get; set; } = string.Empty;
    public UsgsProperties Properties { get; set; } = new();
    public UsgsGeometry? Geometry { get; set; }
}

public class UsgsProperties
{
    public double? Mag { get; set; }
    public string? Place { get; set; }
    public long Time { get; set; }
    public string? Url { get; set; }
    public string? Title { get; set; }
    public string? Type { get; set; }
}

public class UsgsGeometry
{
    // [longitud, latitud, profundidad]
    public List<double> Coordinates { get; set; } = new();
}
