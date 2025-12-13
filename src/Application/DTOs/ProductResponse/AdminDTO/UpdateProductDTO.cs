using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.ProductResponse.AdminDTO
{
    public class UpdateProductDTO
    {
        [StringLength(50, ErrorMessage = "El título no puede exceder los 50 caracteres.")]
        [MinLength(3, ErrorMessage = "El título debe tener al menos 3 caracteres.")]
        public string? Title { get; set; }
        [StringLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres.")]
        [MinLength(10, ErrorMessage = "La descripción debe tener al menos 10 caracteres.")]
        public string? Description { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "El precio debe ser un valor entero positivo.")]
        public decimal? Price { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser un valor positivo.")]
        public int? Stock { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public List<IFormFile>? Images { get; set; } = new List<IFormFile>();
    }
}