using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.DTOs.BrandResponse;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;

namespace TiendaUCN.src.API.Controllers;

public class BrandController : BaseController
{
    private readonly DataContext _context;

    public BrandController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("admin/brands")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllForAdminAsync()
    {
        var list = await _context.Brands.AsNoTracking().OrderBy(b => b.Name).ToListAsync();
        return Ok(new GenericResponse<IEnumerable<Brand>>("Marcas obtenidas", list));
    }

    [HttpGet("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByIdForAdminAsync(int id)
    {
        var brand = await _context.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        if (brand == null) throw new KeyNotFoundException("Marca no encontrada.");
        return Ok(new GenericResponse<Brand>("Marca obtenida", brand));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateBrandDTO dto)
    {
        var exists = await _context.Brands.AnyAsync(b => b.Name.ToLower() == dto.Name.ToLower());
        if (exists) throw new InvalidOperationException("Ya existe una marca con ese nombre.");
        var brand = new Brand { Name = dto.Name };
        await _context.Brands.AddAsync(brand);
        await _context.SaveChangesAsync();
        return Created($"/api/brand/admin/{brand.Id}", new GenericResponse<string>("Marca creada", brand.Id.ToString()));
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateBrandDTO dto)
    {
        var brand = await _context.Brands.FindAsync(id) ?? throw new KeyNotFoundException("Marca no encontrada.");
        var exists = await _context.Brands.AnyAsync(b => b.Id != id && b.Name.ToLower() == dto.Name.ToLower());
        if (exists) throw new InvalidOperationException("Otra marca con ese nombre ya existe.");
        brand.Name = dto.Name;
        await _context.SaveChangesAsync();
        return Ok(new GenericResponse<string>("Marca actualizada", brand.Id.ToString()));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var brand = await _context.Brands.FindAsync(id) ?? throw new KeyNotFoundException("Marca no encontrada.");
        var inUse = await _context.Products.AnyAsync(p => p.BrandId == id);
        if (inUse) throw new InvalidOperationException("No se puede eliminar la marca porque hay productos asociados.");
        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync();
        return Ok(new GenericResponse<string>("Marca eliminada", id.ToString()));
    }
}