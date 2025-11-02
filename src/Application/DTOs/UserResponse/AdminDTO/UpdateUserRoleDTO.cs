using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.UserResponse.AdminDTO
{
    public class UpdateUserRoleDTO
    {
        [Required(ErrorMessage = "El nuevo rol es obligatorio.")]
        [RegularExpression(
            @"^(Admin|Customer)$",
            ErrorMessage = "El rol debe ser Admin o Customer."
        )]
        public required string NewRole { get; set; }
    }
}