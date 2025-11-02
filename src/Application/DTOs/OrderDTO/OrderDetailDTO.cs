using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TiendaUCN.src.Application.DTOs.OrderDTO
{
    public class OrderDetailDTO
    {
        public required string Code { get; set; }
        public required string Total { get; set; }
        public required string SubTotal { get; set; }
        public required string Status { get; set; }
        public required DateTime PurchasedAt { get; set; }
        public required List<OrderItemDTO> Items { get; set; }
    }
}