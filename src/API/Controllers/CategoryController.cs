using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.DTOs.CategoryResponse;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;

namespace TiendaUCN.src.API.Controllers;

public class CategoryController : BaseController
{
    private readonly DataContext _context;

    public CategoryController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("admin/categories")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllForAdminAsync()
    {
        var list = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        return Ok(new GenericResponse<IEnumerable<Category>>("Categorias obtenidas", list));
    }

    [HttpGet("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByIdForAdminAsync(int id)
    {
        var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) throw new KeyNotFoundException("Categoría no encontrada.");
        return Ok(new GenericResponse<Category>("Categoría obtenida", category));
    }

    [HttpPost]
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

    [HttpPatch("{id}")]
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

    [HttpDelete("{id}")]
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
}