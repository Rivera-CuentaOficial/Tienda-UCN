using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TiendaUCN.src.Application.DTOs.CartDTO
{
    public class CartDTO
    {
        public required string BuyerId { get; set; }
        public required int? UserId { get; set; }
        public required List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();
        public required string SubTotalPrice { get; set; }
        public required string TotalPrice { get; set; }
    }
}