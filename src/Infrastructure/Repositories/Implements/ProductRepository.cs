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

    public async Task<int> CreateAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return product.Id;
    }

    public async Task<Brand> CreateOrGetBrandAsync(string brandName)
    {
        var brand = await _context
            .Brands.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Name.ToLower() == brandName.ToLower());

        if (brand != null)
        {
            return brand;
        }
        brand = new Brand { Name = brandName };
        await _context.Brands.AddAsync(brand);
        await _context.SaveChangesAsync();
        return brand;
    }

    public async Task<Category> CreateOrGetCategoryAsync(string categoryName)
    {
        var category = await _context
            .Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());

        if (category != null)
        {
            return category;
        }
        category = new Category { Name = categoryName };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        return category;
    }

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

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context
            .Products.AsNoTracking()
            .Where(p => p.Id == id && p.IsAvailable)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .FirstOrDefaultAsync();
    }

    public async Task<Product?> GetByIdForAdminAsync(int id)
    {
        return await _context
            .Products.AsNoTracking()
            .Where(p => p.Id == id)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .FirstOrDefaultAsync();
    }

    public async Task ToggleActiveAsync(int id)
    {
        await _context
            .Products.Where(p => p.Id == id)
            .ExecuteUpdateAsync(p => p.SetProperty(p => p.IsAvailable, p => !p.IsAvailable));
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
        Product? product =
            await _context.Products.FindAsync(productId)
            ?? throw new KeyNotFoundException("Producto no encontrado");
        product.Stock = stock;
        await _context.SaveChangesAsync();
    }
}