using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.DTOs.ProductResponse;
using TiendaUCN.src.Application.DTOs.ProductResponse.AdminDTO;
using TiendaUCN.src.Application.DTOs.ProductResponse.CustomerDTO;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.API.Controllers;

public class ProductsController : BaseController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Obtiene todos los productos para el cliente con filtros.
    /// </summary>
    /// <param name="searchParams">Parámetros de búsqueda para filtrar los productos.</param>
    /// <returns>Una lista de productos filtrados para el cliente.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    [HttpGet("")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllForCustomerAsync(
        [FromQuery] SearchParamsDTO searchParams
    )
    {
        var result = await _productService.GetFilteredForCustomerAsync(searchParams);
        if (result == null || result.Products.Count == 0)
        {
            throw new KeyNotFoundException("No se encontraron productos.");
        }
        return Ok(
            new GenericResponse<ListedProductsForCustomerDTO>(
                "Productos obtenidos exitosamente",
                result
            )
        );
    }

    /// <summary>
    /// Obtiene un producto por su ID para el cliente.
    /// </summary>
    /// <param name="id">El ID del producto.</param>
    /// <returns>El producto correspondiente al ID proporcionado.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIdForCustomerAsync(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        if (result == null)
        {
            throw new KeyNotFoundException("Producto no encontrado.");
        }
        return Ok(new GenericResponse<ProductDetailDTO>("Producto obtenido exitosamente", result));
    }

    [HttpPatch("{id}/toggle-active")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleActiveAsync(int id)
    {
        await _productService.ToggleActiveAsync(id);
        return Ok(
            new GenericResponse<string>(
                "Estado del producto actualizado exitosamente",
                "El estado del producto ha sido cambiado."
            )
        );
    }
}