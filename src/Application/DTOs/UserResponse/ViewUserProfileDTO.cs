namespace TiendaUCN.src.Application.DTOs.UserResponse
{
    public class ViewUserProfileDTO
    {
        /// <summary>
        /// Nombre del usuario
        /// </summary>
        public required string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        public required string LastName { get; set; }

        /// <summary>
        /// Género del usuario
        /// </summary>
        public required string Gender { get; set; }

        /// <summary>
        /// Fecha de nacimiento del usuario
        /// </summary>
        public required DateTime BirthDate { get; set; }

        /// <summary>
        /// RUT del usuario
        /// </summary>
        public required string Rut { get; set; }

        /// <summary>
        /// Email del usuario
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Número de teléfono del usuario
        /// </summary>
        public required string PhoneNumber { get; set; }
    }
}