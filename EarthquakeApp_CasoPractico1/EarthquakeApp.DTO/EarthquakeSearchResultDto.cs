namespace EarthquakeApp.Dto;

/// <summary>Objeto único que responde el API: info del país + terremotos.</summary>
public class EarthquakeSearchResultDto
{
    public string SearchedCountry { get; set; } = string.Empty;
    public CountryDto Country { get; set; } = new();
    public int TotalEarthquakes { get; set; }
    public double? MaxMagnitude { get; set; }
    public double? AverageMagnitude { get; set; }
    public List<EarthquakeDto> Earthquakes { get; set; } = new();
}
