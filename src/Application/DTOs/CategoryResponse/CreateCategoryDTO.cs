using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.CategoryResponse;

public class CreateCategoryDTO
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public required string Name { get; set; }
}