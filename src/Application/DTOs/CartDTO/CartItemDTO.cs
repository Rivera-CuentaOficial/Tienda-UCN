using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TiendaUCN.src.Application.DTOs.CartDTO
{
    public class CartItemDTO
    {
        public required int ProductId { get; set; }
        public required string ProductTitle { get; set; }
        public required string ProductImageUrl { get; set; }
        public required int Price { get; set; }
        public required int Quantity { get; set; }

        public required int Discount { get; set; }

        public required string SubTotalPrice { get; set; }
        public required string TotalPrice { get; set; }
    }
}