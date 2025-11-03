using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Application.DTOs.CategoryResponse.AdminDTO;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageSize;

        public CategoryRepository(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _defaultPageSize =
                _configuration.GetValue<int?>("Categories:DefaultPageSize")
                ?? throw new ArgumentNullException(
                    "Categories:DefaultPageSize configuration is missing.");
        }

        /// <summary>
        /// Obtiene categorías filtradas para administración.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar categorías.</param>
        /// <returns>Lista de categorías filtradas y el conteo total.</returns>
        public async Task<(IEnumerable<Category> categories, int totalCount)> GetFilteredForAdminAsync(CategorySearchTermDTO searchParams)
        {
            var query = _context.Categories.AsQueryable();

            int totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(searchParams.Name))
            {
                query = query.Where(c => c.Name.Contains(searchParams.Name));
            }

            int pageSize = searchParams.PageSize ?? _defaultPageSize;

            var categories = await query
                .Skip((searchParams.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (categories, totalCount);
        }

        /// <summary>
        /// Obtiene una categoría por su ID.
        /// </summary>
        /// <param name="id">ID de la categoría.</param>
        /// <returns>La categoría encontrada o null si no existe.</returns>
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Where(c => c.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
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
    }
}