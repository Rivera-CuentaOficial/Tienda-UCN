using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.BrandResponse;

public class UpdateBrandDTO
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public required string Name { get; set; }
}