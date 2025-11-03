using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.BrandResponse.AdminDTO
{
    public class BrandSearchTermDTO
    {
        /// <summary>
        /// Número de página para la paginación.
        /// </summary>
        [Required(ErrorMessage = "El número de página es obligatorio.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "El número de página debe ser un número entero positivo."
        )]
        public required int PageNumber { get; set; }

        /// <summary>
        /// Tamaño de página para la paginación.
        /// </summary>
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "El tamaño de página debe ser un número entero positivo."
        )]
        public int? PageSize { get; set; }

        /// <summary>
        /// Término de búsqueda para filtrar marcas por nombre.
        /// </summary>
        [MinLength(2, ErrorMessage = "El término de búsqueda debe tener al menos 2 caracteres.")]
        [MaxLength(40, ErrorMessage = "El término de búsqueda no puede exceder los 40 caracteres.")]
        public string? Name { get; set; }
    }
}