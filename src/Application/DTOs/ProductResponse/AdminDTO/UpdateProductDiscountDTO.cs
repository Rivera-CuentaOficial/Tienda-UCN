using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.ProductResponse.AdminDTO
{
    public class UpdateProductDiscountDTO
    {
        [Required(ErrorMessage = "El descuento es obligatorio.")]
        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100.")]
        public required int Discount { get; set; }
    }
}