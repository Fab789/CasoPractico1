namespace EarthquakeApp.Dto;

public class ErrorResponseDto
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
}
