using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Application.DTOs.ProductResponse;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements;

public class ProductRepository : IProductRepository
{
    private readonly DataContext _context;
    private readonly IConfiguration _configuration;
    private readonly int _defaultPageSize;

    public ProductRepository(DataContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _defaultPageSize =
            _configuration.GetValue<int?>("Products:DefaultPageSize")
            ?? throw new ArgumentNullException(
                "El tamaño de página por defecto no puede ser nulo."
            );
    }

    public async Task<int> CreateAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return product.Id;
    }

    public async Task<bool> UpdateAsync(Product existingProduct)
    {
        existingProduct.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForAdminAsync(
        SearchParamsDTO searchParams
    )
    {
        var query = _context
            .Products.Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images.OrderBy(i => i.CreatedAt).Take(1)) // Cargamos la URL de la imagen principal a la hora de crear el producto
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
        {
            var searchTerm = searchParams.SearchTerm.Trim().ToLower();

            query = query.Where(p =>
                p.Title.ToLower().Contains(searchTerm)
                || p.Description.ToLower().Contains(searchTerm)
                || p.Category.Name.ToLower().Contains(searchTerm)
                || p.Brand.Name.ToLower().Contains(searchTerm)
                || p.Status.ToString().ToLower().Contains(searchTerm)
                || p.Price.ToString().ToLower().Contains(searchTerm)
                || p.Stock.ToString().ToLower().Contains(searchTerm)
            );
        }
        var pageSize = searchParams.PageSize ?? _defaultPageSize;
        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((searchParams.PageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();
        int totalCount = await query.CountAsync();
        return (products, totalCount);
    }

    public async Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForCustomerAsync(
        SearchParamsDTO searchParams
    )
    {
        var query = _context
            .Products.Where(p => p.IsAvailable)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images.OrderBy(i => i.CreatedAt).Take(1))
            .AsSplitQuery()
            .AsNoTracking();
        if (query == null)
        {
            throw new KeyNotFoundException("No hay productos disponibles en estos momentos.");
        }

        if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
        {
            var searchTerm = searchParams.SearchTerm.Trim().ToLower();

            query = query.Where(p =>
                p.Title.ToLower().Contains(searchTerm)
                || p.Description.ToLower().Contains(searchTerm)
                || p.Category.Name.ToLower().Contains(searchTerm)
                || p.Brand.Name.ToLower().Contains(searchTerm)
                || p.Status.ToString().ToLower().Contains(searchTerm)
                || p.Price.ToString().ToLower().Contains(searchTerm)
                || p.Stock.ToString().ToLower().Contains(searchTerm)
            );
        }
        int totalCount = await query.CountAsync();
        int pageSize = searchParams.PageSize ?? _defaultPageSize;
        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((searchParams.PageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        return (products, totalCount);
    }

    public async Task<Product?> GetByIdAsync(int id, bool asTracking = false)
    {
        IQueryable<Product> query = _context.Products;
        if (!asTracking) query = query.AsNoTracking();
        return await query
            .Where(p => p.Id == id)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .FirstOrDefaultAsync();
    }

    public async Task<Product?> GetByIdForAdminAsync(int id, bool asTracking = false)
    {
        IQueryable<Product> query = _context.Products;
        if (!asTracking) query = query.AsNoTracking();
        return await query
            .Where(p => p.Id == id)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ToggleActiveAsync(Product product)
    {
        return await _context
            .Products.Where(p => p.Id == product.Id)
            .ExecuteUpdateAsync(p => p
                .SetProperty(p => p.IsAvailable, p => !p.IsAvailable)
                .SetProperty(p => p.UpdatedAt, p => DateTime.UtcNow)
            ) > 0;
    }

    public async Task<int> GetRealStockAsync(int productId)
    {
        return await _context
            .Products.AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => p.Stock)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateStockAsync(int productId, int stock)
    {
        await _context
            .Products
            .Where(p => p.Id == productId)
            .ExecuteUpdateAsync(p => p
                .SetProperty(p => p.Stock, p => stock)
                .SetProperty(p => p.UpdatedAt, p => DateTime.UtcNow)
            );
    }

    public async Task<int> CountProductsByCategoryIdAsync(int categoryId)
    {
        return await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .CountAsync();
    }

    public async Task<int> CountProductsByBrandIdAsync(int brandId)
    {
        return await _context.Products
            .Where(p => p.BrandId == brandId)
            .CountAsync();
    }

    public async Task<bool> DeleteAsync(Product product)
    {
        return await _context
            .Products
            .Where(p => p.Id == product.Id)
            .ExecuteUpdateAsync(p =>
                p
                    .SetProperty(p => p.IsDeleted, p => true)
                    .SetProperty(p => p.DeletedAt, p => DateTime.UtcNow)
            ) > 0;
    }
}