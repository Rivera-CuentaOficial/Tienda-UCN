using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.Application.DTOs.AuthResponse
{
    public class ResendVerifyDTO
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public required string Email { get; set; }
    }
}