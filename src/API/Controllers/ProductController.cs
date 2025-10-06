using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.DTOs.ProductResponse;
using TiendaUCN.src.Application.DTOs.ProductResponse.Admin;
using TiendaUCN.src.Application.DTOs.ProductResponse.Customer;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.API.Controllers;

public class ProductController : BaseController
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("admin/products")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllForAdminAsync([FromQuery] SearchParamsDTO searchParams)
    {
        var result = await _productService.GetFilteredForAdminAsync(searchParams);
        if (result == null || result.Products.Count == 0)
        {
            throw new KeyNotFoundException("No se encontraron productos.");
        }
        return Ok(
            new GenericResponse<ListedProductsForAdminDTO>(
                "Productos obtenidos exitosamente",
                result
            )
        );
    }

    [HttpGet("customer/products")]
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

    [HttpGet("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByIdForAdminAsync(int id)
    {
        var result = await _productService.GetByIdForAdminAsync(id);
        if (result == null)
        {
            throw new KeyNotFoundException("Producto no encontrado.");
        }
        return Ok(new GenericResponse<ProductDetailDTO>("Producto obtenido exitosamente", result));
    }

    [HttpPost()]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAsync([FromForm] CreateProductDTO createProductDTO)
    {
        var result = await _productService.CreateAsync(createProductDTO);
        return Created(
            $"/api/product/{result}",
            new GenericResponse<string>("Producto creado exitosamente", result)
        );
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