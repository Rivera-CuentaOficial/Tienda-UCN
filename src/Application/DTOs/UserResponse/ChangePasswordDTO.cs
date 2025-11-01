using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.UserResponse
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "La contraseña es requerida.")]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ])(?=.*[!@#$%^&*()_+\[\]{};':""\\|,.<>/?]).*$",
            ErrorMessage = "La contraseña debe ser alfanumérica y contener al menos una mayúscula y al menos un caracter especial."
        )]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [MaxLength(20, ErrorMessage = "La contraseña debe tener como máximo 20 caracteres")]
        public required string CurrentPassword { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ])(?=.*[!@#$%^&*()_+\[\]{};':""\\|,.<>/?]).*$",
            ErrorMessage = "La contraseña debe ser alfanumérica y contener al menos una mayúscula y al menos un caracter especial."
        )]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [MaxLength(20, ErrorMessage = "La contraseña debe tener como máximo 20 caracteres")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ])(?=.*[!@#$%^&*()_+\[\]{};':""\\|,.<>/?]).*$",
            ErrorMessage = "La contraseña debe ser alfanumérica y contener al menos una mayúscula y al menos un caracter especial."
        )]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [MaxLength(20, ErrorMessage = "La contraseña debe tener como máximo 20 caracteres")]
        public required string ConfirmNewPassword { get; set; }
    }
}