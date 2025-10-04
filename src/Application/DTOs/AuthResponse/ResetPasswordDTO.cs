using System.ComponentModel.DataAnnotations;
using System.Runtime.Versioning;

namespace TiendaUCN.Application.DTOs.AuthResponse;

public class ResetPasswordDTO
{
    /// <summary>
    /// Email del usuario
    /// </summary>
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato valido.")]
    public required string Email { get; set; }

    /// <summary>
    /// Codigo de verificacion para el reseteo de contraseña
    /// </summary>
    [Required(ErrorMessage = "El codigo es obligatorio.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "El código debe tener 6 dígitos.")]
    public required string Code { get; set; }

    /// <summary>
    /// Nueva contraseña del usuario
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida.")]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ])(?=.*[!@#$%^&*()_+\[\]{};':""\\|,.<>/?]).*$",
        ErrorMessage = "La contraseña debe ser alfanumérica y contener al menos una mayúscula y al menos un caracter especial."
    )]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    [MaxLength(20, ErrorMessage = "La contraseña debe tener como máximo 20 caracteres")]
    public required string NewPassword { get; set; }

    /// <summary>
    /// Confirmacion de la nueva contraseña del usuario
    /// </summary>
    [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
    [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
    public required string ConfirmNewPassword { get; set; }
}