using System.ComponentModel.DataAnnotations;

namespace EarthquakeApp.Web.Models;

public class SearchViewModel
{
    [Required(ErrorMessage = "Escribe el nombre de un país.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Solo se permiten letras y espacios (sin números ni caracteres especiales).")]
    public string Country { get; set; } = string.Empty;
}
