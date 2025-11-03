using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.CategoryResponse;

public class CreateCategoryDTO
{
    [Required(ErrorMessage = "El nombre de la categoría es requerido.")]
    [StringLength(50, MinimumLength = 2)]
    public required string Name { get; set; }
    [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres.")]
    public string? Description { get; set; }
}