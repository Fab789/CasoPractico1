namespace EarthquakeApp.Dto;

public class EarthquakeDto
{
    public int Id { get; set; }
    public string UsgsId { get; set; } = string.Empty;
    public double? Magnitude { get; set; }
    public string Place { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime TimeUtc { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? DepthKm { get; set; }
    public string? Url { get; set; }
    public string? Title { get; set; }
    public string? Type { get; set; }
}
