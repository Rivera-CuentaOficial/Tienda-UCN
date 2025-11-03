using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.DTOs.BrandResponse;
using TiendaUCN.src.Application.DTOs.BrandResponse.AdminDTO;
using TiendaUCN.src.Application.DTOs.CategoryResponse;
using TiendaUCN.src.Application.DTOs.CategoryResponse.AdminDTO;
using TiendaUCN.src.Application.DTOs.ProductResponse;
using TiendaUCN.src.Application.DTOs.ProductResponse.AdminDTO;
using TiendaUCN.src.Application.DTOs.ProductResponse.CustomerDTO;
using TiendaUCN.src.Application.DTOs.UserResponse;
using TiendaUCN.src.Application.DTOs.UserResponse.AdminDTO;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;

namespace TiendaUCN.src.API.Controllers
{
    public class AdminController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly DataContext _context;

        public AdminController(IUserService userService, IProductService productService, ICategoryService categoryService, IBrandService brandService, DataContext context)
        {
            _userService = userService;
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _context = context;
        }

        #region User Management

        /// <summary>
        /// Obtener todos los usuarios con filtros para el administrador.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar usuarios.</param>
        /// <returns>Lista de usuarios filtrados.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUserAsync([FromQuery] UserSearchParamsDTO searchParams)
        {
            var result = await _userService.GetFilteredForAdminAsync(searchParams);
            if (result == null || result.Users.Count == 0)
            {
                throw new KeyNotFoundException("No se encontraron usuarios.");
            }
            return Ok(
                new GenericResponse<ListedUsersForAdminDTO>(
                    "Usuarios obtenidos exitosamente",
                    result
                )
            );
        }

        /// <summary>
        /// Obtener un usuario por su ID.
        /// </summary>
        /// <param name="id">ID del usuario.</param>
        /// <returns>Detalles del usuario.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserByIdAsync(int id)
        {
            var result = await _userService.GetByIdAsync(id);
            if (result == null)
            {
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            return Ok(
                new GenericResponse<UserDetailsForAdminDTO>(
                    "Usuario obtenido exitosamente",
                    result
                )
            );
        }

        /// <summary>
        /// Actualiza el estado de un usuario (activo/inactivo).
        /// </summary>
        /// <param name="id">El ID del usuario.</param>
        /// <param name="updateUserStatusDTO">El DTO con la información del nuevo estado del usuario.</param>
        /// <returns>El estado del usuario actualizado.</returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        [HttpPatch("users/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserStatusAsync(int id, [FromBody] UpdateUserStatusDTO updateUserStatusDTO)
        {
            var adminId =
                (
                    User.Identity?.IsAuthenticated == true
                        ? User
                            .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                            ?.Value
                        : null
                ) ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
            int.TryParse(adminId, out int parsedAdminId);
            var result = await _userService.UpdateUserStatusAsync(parsedAdminId, id, updateUserStatusDTO);
            return Ok(
                new GenericResponse<string>("Estado del usuario actualizado exitosamente", result)
            );
        }

        /// <summary>
        /// Actualiza el rol de un usuario.
        /// </summary>
        /// <param name="id">El ID del usuario.</param>
        /// <param name="updateUserRoleDTO">El DTO con la información del nuevo rol del usuario.</param>
        /// <returns>El rol del usuario actualizado.</returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        [HttpPatch("users/{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRoleAsync(int id, [FromBody] UpdateUserRoleDTO updateUserRoleDTO)
        {
            var adminId =
                (
                    User.Identity?.IsAuthenticated == true
                        ? User
                            .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                            ?.Value
                        : null
                ) ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
            int.TryParse(adminId, out int parsedAdminId);
            var result = await _userService.UpdateUserRoleAsync(parsedAdminId, id, updateUserRoleDTO);
            return Ok(
                new GenericResponse<string>("Rol del usuario actualizado exitosamente", result)
            );
        }

        #endregion
        #region Product Management

        /// <summary>
        /// Obtiene todos los productos con filtros para el administrador.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar los productos.</param>
        /// <returns>Una lista de productos filtrados para el administrador.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        [HttpGet("products")]
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

        /// <summary>
        /// Obtiene un producto por su ID para el administrador.
        /// </summary>
        /// <param name="id">El ID del producto.</param>
        /// <returns>El producto correspondiente al ID proporcionado.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        [HttpGet("products/{id}")]
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

        /// <summary>
        /// Crea un nuevo producto.
        /// </summary>
        /// <param name="createProductDTO">Los datos del producto a crear.</param>
        /// <returns>El ID del producto creado.</returns>
        [HttpPost("products")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync([FromForm] CreateProductDTO createProductDTO)
        {
            var result = await _productService.CreateAsync(createProductDTO);
            return Created(
                $"/api/product/{result}",
                new GenericResponse<string>("Producto creado exitosamente", result)
            );
        }

        #endregion
        #region Category Management

        [HttpGet("categories")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCategoryFilteredForAdminAsync([FromQuery] CategorySearchTermDTO categorySearchTermDTO)
        {
            var result = await _categoryService.GetFilteredForAdminAsync(categorySearchTermDTO);
            if (result == null || result.Categories.Count == 0)
            {
                throw new KeyNotFoundException("No se encontraron categorías.");
            }
            //var list = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            return Ok(
                new GenericResponse<CategoriesForAdminDTO>(
                    "Categorias obtenidas",
                    result
                ));
        }

        [HttpGet("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCategoryByIdForAdminAsync(int id)
        {
            var result = await _categoryService.GetByIdForAdminAsync(id);
            if (result == null)
            {
                throw new KeyNotFoundException("Categoría no encontrada.");
            }
            return Ok(new GenericResponse<CategoryDetailDTO>("Categoría obtenida", result));
        }

        [HttpPost("categories")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateCategoryDTO dto)
        {
            var exists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());
            if (exists) throw new InvalidOperationException("Ya existe una categoría con ese nombre.");
            var category = new Category { Name = dto.Name };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return Created($"/api/category/admin/{category.Id}", new GenericResponse<string>("Categoría creada", category.Id.ToString()));
        }

        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateCategoryDTO dto)
        {
            var category = await _context.Categories.FindAsync(id) ?? throw new KeyNotFoundException("Categoría no encontrada.");
            var exists = await _context.Categories.AnyAsync(c => c.Id != id && c.Name.ToLower() == dto.Name.ToLower());
            if (exists) throw new InvalidOperationException("Otra categoría con ese nombre ya existe.");
            category.Name = dto.Name;
            await _context.SaveChangesAsync();
            return Ok(new GenericResponse<string>("Categoría actualizada", category.Id.ToString()));
        }

        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id) ?? throw new KeyNotFoundException("Categoría no encontrada.");
            var inUse = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (inUse) throw new InvalidOperationException("No se puede eliminar la categoría porque hay productos asociados.");
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok(new GenericResponse<string>("Categoría eliminada", id.ToString()));
        }
        #endregion
        #region Brand Management

        [HttpGet("brands")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBrandFilteredForAdminAsync([FromQuery] BrandSearchTermDTO brandSearchTermDTO)
        {
            var result = await _brandService.GetFilteredForAdminAsync(brandSearchTermDTO);
            if (result == null || result.Brands.Count == 0)
            {
                throw new KeyNotFoundException("No se encontraron marcas.");
            }
            //var list = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            return Ok(
                new GenericResponse<BrandsForAdminDTO>(
                    "Marcas obtenidas",
                    result
                ));
        }

        [HttpGet("brands/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBrandByIdForAdminAsync(int id)
        {
            var result = await _brandService.GetByIdForAdminAsync(id);
            if (result == null)
            {
                throw new KeyNotFoundException("Marca no encontrada.");
            }
            return Ok(new GenericResponse<BrandDetailDTO>("Marca obtenida", result));
        }

        [HttpPost("brands")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateBrandDTO dto)
        {
            var exists = await _context.Brands.AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());
            if (exists) throw new InvalidOperationException("Ya existe una marca con ese nombre.");
            var brand = new Brand { Name = dto.Name };
            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();
            return Created($"/api/brand/admin/{brand.Id}", new GenericResponse<string>("Marca creada", brand.Id.ToString()));
        }

        [HttpPut("brands/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateBrandDTO dto)
        {
            var brand = await _context.Brands.FindAsync(id) ?? throw new KeyNotFoundException("Marca no encontrada.");
            var exists = await _context.Brands.AnyAsync(c => c.Id != id && c.Name.ToLower() == dto.Name.ToLower());
            if (exists) throw new InvalidOperationException("Otra marca con ese nombre ya existe.");
            brand.Name = dto.Name;
            await _context.SaveChangesAsync();
            return Ok(new GenericResponse<string>("Marca actualizada", brand.Id.ToString()));
        }

        [HttpDelete("brands/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBrandAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id) ?? throw new KeyNotFoundException("Marca no encontrada.");
            var inUse = await _context.Products.AnyAsync(p => p.BrandId == id);
            if (inUse) throw new InvalidOperationException("No se puede eliminar la marca porque hay productos asociados.");
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return Ok(new GenericResponse<string>("Marca eliminada", id.ToString()));
        }
        #endregion
    }
}