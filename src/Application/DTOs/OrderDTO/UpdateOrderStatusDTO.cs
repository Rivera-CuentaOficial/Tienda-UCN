using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.OrderDTO
{
    public class UpdateOrderStatusDTO
    {
        [Required]
        [MinLength(3)]
        public string Status { get; set; } = null!;
    }
}