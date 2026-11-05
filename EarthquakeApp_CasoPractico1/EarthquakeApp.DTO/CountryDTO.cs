namespace EarthquakeApp.Dto;

public class CountryDto
{
    public string CommonName { get; set; } = string.Empty;
    public string OfficialName { get; set; } = string.Empty;
    public string Cca2 { get; set; } = string.Empty;
    public string Capital { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Subregion { get; set; } = string.Empty;
    public long Population { get; set; }
    public double Area { get; set; }
    public string FlagUrl { get; set; } = string.Empty;
    public List<string> Languages { get; set; } = new();
    public List<string> Currencies { get; set; } = new();
    public List<string> Timezones { get; set; } = new();
}
