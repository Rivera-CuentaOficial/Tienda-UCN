using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TiendaUCN.src.Application.DTOs.OrderDTO
{
    public class ListedOrderDetailDTO
    {
        public required List<OrderDetailDTO> Orders { get; set; } = new List<OrderDetailDTO>();
        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }
    }
}
