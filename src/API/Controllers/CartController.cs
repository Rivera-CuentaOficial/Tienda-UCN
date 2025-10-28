using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.DTOs.CartDTO;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.API.Controllers
{
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;

        private CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Obtiene el carrito de compras del comprador actual.
        /// </summary>
        /// <returns>El carrito de compras del comprador actual.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCart()
        {
            var buyerId = GetBuyerId();
            var userId =
                User.Identity?.IsAuthenticated == true
                    ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                    : null;
            int? parsedUserId = userId != null && int.TryParse(userId, out int id) ? id : null;
            var cart = await _cartService.CreateOrGetAsync(buyerId, parsedUserId);
            return Ok(new GenericResponse<CartDTO>("Carrito obtenido exitosamente", cart));
        }

        /// <summary>
        /// Agrega un item al carrito de compras.
        /// </summary>
        /// <param name="addCartItemDTO">El DTO con la información del item a agregar.</param>
        /// <returns>El carrito de compras actualizado.</returns>
        [HttpPost("items")]
        [AllowAnonymous]
        public async Task<IActionResult> AddItem([FromForm] AddCartItemDTO addCartItemDTO)
        {
            var buyerId = GetBuyerId();
            var userId =
                User.Identity?.IsAuthenticated == true
                    ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                    : null;
            var parsedUserId = userId != null && int.TryParse(userId, out int id) ? id : (int?)null;
            var result = await _cartService.AddItemAsync(
                buyerId,
                addCartItemDTO.ProductId,
                addCartItemDTO.Quantity,
                parsedUserId
            );
            return Ok(new GenericResponse<CartDTO>("Item agregado exitosamente", result));
        }

        /// <summary>
        /// Elimina un item del carrito de compras.
        /// </summary>
        /// <param name="productId">El ID del producto a eliminar.</param>
        /// <returns>El carrito de compras actualizado.</returns>
        [HttpDelete("items/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var buyerId = GetBuyerId();
            var userId =
                User.Identity?.IsAuthenticated == true
                    ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                    : null;
            var parsedUserId = userId != null && int.TryParse(userId, out int id) ? id : (int?)null;
            var result = await _cartService.RemoveItemAsync(buyerId, productId, parsedUserId);
            return Ok(new GenericResponse<CartDTO>("Item eliminado exitosamente", result));
        }

        /// <summary>
        /// Limpia el carrito de compras del comprador actual.
        /// </summary>
        /// <returns>El carrito de compras actualizado.</returns>
        [HttpPost("clear")]
        [AllowAnonymous]
        public async Task<IActionResult> ClearCart()
        {
            var buyerId = GetBuyerId();
            var userId =
                User.Identity?.IsAuthenticated == true
                    ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                    : null;
            var parsedUserId = userId != null && int.TryParse(userId, out int id) ? id : (int?)null;
            var result = await _cartService.ClearAsync(buyerId, parsedUserId);
            return Ok(new GenericResponse<CartDTO>("Carrito limpiado exitosamente", result));
        }

        /// <summary>
        /// Actualiza la cantidad de un item en el carrito de compras.
        /// </summary>
        /// <param name="changeItemQuantityDTO">Id del item y cantidad nueva</param>
        /// <returns>Resultado con el carrito de compras actualizado.</returns>
        [HttpPatch("items")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateItemQuantity(
            [FromForm] ChangeItemQuantityDTO changeItemQuantityDTO
        )
        {
            var buyerId = GetBuyerId();
            var userId =
                User.Identity?.IsAuthenticated == true
                    ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                    : null;
            var parsedUserId = userId != null && int.TryParse(userId, out int id) ? id : (int?)null;
            var result = await _cartService.UpdateItemQuantityAsync(
                buyerId,
                changeItemQuantityDTO.ProductId,
                changeItemQuantityDTO.Quantity,
                parsedUserId
            );
            return Ok(
                new GenericResponse<CartDTO>("Cantidad de item actualizada exitosamente", result)
            );
        }

        /// <summary>
        /// Realiza el checkout del carrito de compras.
        /// </summary>
        /// <returns>El carrito de compras actualizado.</returns>
        [HttpPost("checkout")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CheckoutAsync()
        {
            var buyerId = GetBuyerId();
            var userId =
                User.Identity?.IsAuthenticated == true
                    ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                    : null;
            var parsedUserId = userId != null && int.TryParse(userId, out int id) ? id : (int?)null;
            var result = await _cartService.CheckoutAsync(buyerId, parsedUserId);
            return Ok(new GenericResponse<CartDTO>("Checkout realizado exitosamente", result));
        }

        /// <summary>
        /// Método helper para obtener buyerId del contexto
        /// </summary>
        private string GetBuyerId()
        {
            var buyerId = HttpContext.Items["BuyerId"]?.ToString();

            if (string.IsNullOrEmpty(buyerId))
            {
                throw new Exception("No se encontró el id del comprador.");
            }
            return buyerId;
        }
    }
}