namespace EarthquakeApp.Api.Exceptions;

public class CountryNotFoundException : Exception
{
    public CountryNotFoundException(string country)
        : base($"No se encontró ningún país con el nombre \"{country}\".") { }
}

public class ExternalApiException : Exception
{
    public ExternalApiException(string message, Exception? inner = null) : base(message, inner) { }
}