using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.UserResponse.AdminDTO
{
    public class UpdateUserStatusDTO
    {
        [Required(ErrorMessage = "El estado del usuario es obligatorio.")]
        [RegularExpression(
            @"^(Active|Blocked)$",
            ErrorMessage = "El estado debe ser Active o Blocked."
        )]
        public required string Status { get; set; }

        [MaxLength(200, ErrorMessage = "La razón no puede exceder los 200 caracteres.")]
        public string? Reason { get; set; }
    }
}