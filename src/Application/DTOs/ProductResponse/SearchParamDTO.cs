using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.ProductResponse;

public class SearchParamsDTO
{
    // Hacemos que PageNumber tenga un valor por defecto (1) para evitar errores de validación
    // cuando el cliente no lo proporciona en la query string.
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "El número de página debe ser un número entero positivo."
    )]
    public int PageNumber { get; set; } = 1;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "El tamaño de página debe ser un número entero positivo."
    )]
    public int? PageSize { get; set; }

    [MinLength(2, ErrorMessage = "El término de búsqueda debe tener al menos 2 caracteres.")]
    [MaxLength(40, ErrorMessage = "El término de búsqueda no puede exceder los 40 caracteres.")]
    public string? SearchTerm { get; set; }
}