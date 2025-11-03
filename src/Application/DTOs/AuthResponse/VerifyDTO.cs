using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.AuthResponse;

public class VerifyDTO
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "El código de verificacion es obligatorio")]
    [RegularExpression(
        @"^\d{6}$",
        ErrorMessage = "El código de verificación debe tener 6 dígitos."
    )]
    public required string VerificationCode { get; set; }
}