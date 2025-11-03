using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Application.DTOs.BrandResponse.AdminDTO;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class BrandRepository : IBrandRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageSize;

        public BrandRepository(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _defaultPageSize =
                _configuration.GetValue<int?>("Brands:DefaultPageSize")
                ?? throw new ArgumentNullException(
                    "Brands:DefaultPageSize configuration is missing.");
        }

        /// <summary>
        /// Obtiene marcas filtradas para administración.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar marcas.</param>
        /// <returns>Lista de marcas filtradas y el conteo total.</returns>
        public async Task<(IEnumerable<Brand> brands, int totalCount)> GetFilteredForAdminAsync(BrandSearchTermDTO searchParams)
        {
            var query = _context.Brands.AsQueryable();

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
        /// Obtiene una marca por su ID.
        /// </summary>
        /// <param name="id">ID de la marca.</param>
        /// <returns>La marca encontrada o null si no existe.</returns>
        public async Task<Brand?> GetByIdAsync(int id)
        {
            return await _context.Brands
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
    }
}