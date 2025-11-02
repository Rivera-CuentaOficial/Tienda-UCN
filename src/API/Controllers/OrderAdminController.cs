using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.DTOs.OrderDTO;
using TiendaUCN.src.Application.DTOs.ProductResponse;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/order/admin")]
    public class OrderAdminController : BaseController
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;

        public OrderAdminController(IOrderRepository orderRepository, IOrderService orderService)
        {
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders([FromQuery] SearchParamsDTO searchParams)
        {
            var (orders, totalCount) = await _orderRepository.GetFilteredForAdminAsync(searchParams);
            var defaultPageSize = searchParams.PageSize ?? 10;
            var totalPages = (int)Math.Ceiling((double)totalCount / defaultPageSize);
            int currentPage = searchParams.PageNumber;
            int pageSize = defaultPageSize;
            if (currentPage < 1 || (totalPages > 0 && currentPage > totalPages))
            {
                return BadRequest(new GenericResponse<string>("Número de página fuera de rango", null));
            }

            var listed = new ListedOrderDetailDTO
            {
                Orders = orders.Adapt<List<OrderDetailDTO>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize,
            };

            return Ok(new GenericResponse<ListedOrderDetailDTO>("Órdenes obtenidas", listed));
        }

        [HttpGet("{orderCode}")]
        public async Task<IActionResult> GetOrderDetail(string orderCode)
        {
            var result = await _orderService.GetDetailAsync(orderCode);
            return Ok(new GenericResponse<OrderDetailDTO>("Detalle de orden obtenido", result));
        }

        [HttpPatch("{orderCode}/status")]
        public async Task<IActionResult> UpdateStatus(string orderCode, [FromBody] UpdateOrderStatusDTO dto)
        {
            await _orderService.UpdateOrderStatusAsync(orderCode, dto.Status);
            return Ok(new GenericResponse<string>("Estado de orden actualizado", orderCode));
        }
    }
}