using System.ComponentModel.DataAnnotations;
using TiendaUCN.src.Application.Validators;

namespace TiendaUCN.src.Application.DTOs.UserResponse
{
    public class UpdateProfileDTO
    {
        /// <summary>
        /// Nombre del usuario
        /// </summary>
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$",
            ErrorMessage = "El Nombre solo puede contener carácteres del abecedario español."
        )]
        [MinLength(2, ErrorMessage = "El nombre debe tener mínimo 2 letras.")]
        [MaxLength(50, ErrorMessage = "El nombre debe tener máximo 50 letras.")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$",
            ErrorMessage = "El Apellido solo puede contener carácteres del abecedario español."
        )]
        [MinLength(2, ErrorMessage = "El apellido debe tener mínimo 2 letras.")]
        [MaxLength(50, ErrorMessage = "El apellido debe tener máximo 50 letras.")]
        public string? LastName { get; set; }

        /// <summary>
        /// Género del usuario
        /// </summary>
        [RegularExpression(
            @"^(Masculino|Femenino|Otro)$",
            ErrorMessage = "El género debe ser Masculino, Femenino u Otro."
        )]
        public string? Gender { get; set; }

        /// <summary>
        /// Fecha de nacimiento del usuario
        /// </summary>
        [BirthDateValidation]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Número de teléfono del usuario
        /// </summary>
        [RegularExpression(
            @"^\d{9}$",
            ErrorMessage = "El número de teléfono debe tener 9 dígitos."
        )]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Email del usuario
        /// </summary>
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public string? Email { get; set; }

        /// <summary>
        /// Código de verificación para cambio de email
        /// </summary>
        public int? VerificationCode { get; set; }

        /// <summary>
        /// RUT del usuario
        /// </summary>
        [RegularExpression(
            @"^$|^\d{7,8}-[0-9kK]$",
            ErrorMessage = "El Rut debe tener formato XXXXXXXX-X"
        )]
        [RutValidation(ErrorMessage = "El Rut no es válido.")]
        public string? Rut { get; set; }
    }
}