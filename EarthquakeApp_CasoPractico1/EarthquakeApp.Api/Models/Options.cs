namespace EarthquakeApp.Api.Models;

public class UsgsOptions
{
    public string BaseUrl { get; set; } = "https://earthquake.usgs.gov/fdsnws/event/1/query";
    public string StartTime { get; set; } = "2020-01-01";
    public double MinMagnitude { get; set; } = 5;
    public int CacheMinutes { get; set; } = 10;
}

public class RestCountriesOptions
{
    public string BaseUrl { get; set; } = "https://api.restcountries.com/countries/v5/";
    public string ApiKey { get; set; } = string.Empty;
    public int CacheMinutes { get; set; } = 60;
}
