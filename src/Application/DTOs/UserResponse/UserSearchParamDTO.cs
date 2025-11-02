using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.UserResponse;

public class UserSearchParamsDTO
{
    /// <summary>
    /// Número de página para la paginación.
    /// </summary>
    [Required(ErrorMessage = "El número de página es obligatorio.")]
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "El número de página debe ser un número entero positivo."
    )]
    public int PageNumber { get; set; }

    /// <summary>
    /// Tamaño de página para la paginación.
    /// </summary>
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "El tamaño de página debe ser un número entero positivo."
    )]
    public int? PageSize { get; set; }

    /// <summary>
    /// Término de búsqueda para filtrar usuarios por email.
    /// </summary>
    [MinLength(2, ErrorMessage = "El término de búsqueda debe tener al menos 2 caracteres.")]
    [MaxLength(40, ErrorMessage = "El término de búsqueda no puede exceder los 40 caracteres.")]
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Rol del usuario para filtrar (Admin o Customer).
    /// </summary>
    [RegularExpression(
        @"^(Admin|Customer)$",
        ErrorMessage = "El rol debe ser Admin o Customer."
    )]
    public string? RoleName { get; set; }

    /// <summary>
    /// Estado del usuario para filtrar (Active o Blocked).
    /// </summary>
    [RegularExpression(
        @"^(Active|Blocked)$",
        ErrorMessage = "El estado debe ser Active o Blocked."
    )]
    public string? State { get; set; }

    [RegularExpression(
        @"^(ascending|descending)$",
        ErrorMessage = "El orden debe ser ascending o descending."
    )]
    public string? SortOrder { get; set; }

    [RegularExpression(
        @"^(email|lastlogin|registeredat)$",
        ErrorMessage = "El campo SortBy debe ser email, lastlogin o registeredat."
    )]
    public string? SortBy { get; set; }
}