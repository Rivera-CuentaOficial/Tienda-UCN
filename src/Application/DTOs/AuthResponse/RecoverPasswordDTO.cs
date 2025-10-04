using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.Application.DTOs.AuthResponse;

public class RecoverPasswordDTO
{
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El Correo electrónico no es válido.")]
    public required string Email { get; set; }
}