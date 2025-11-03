using Mapster;
using Serilog;
using TiendaUCN.src.Application.DTOs.BrandResponse.AdminDTO;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IProductRepository _productRepository;
        private int _defaultPageSize;

        public BrandService(IBrandRepository brandRepository, IProductRepository productRepository, IConfiguration configuration)
        {
            _brandRepository = brandRepository;
            _productRepository = productRepository;
            _defaultPageSize =
                configuration.GetValue<int?>("Brands:DefaultPageSize")
                ?? throw new ArgumentNullException(
                    "La configuración para el tamaño de página por defecto de marcas no está establecida.");
        }

        /// <summary>
        /// Obtiene marcas filtradas para el panel de administración.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar marcas.</param>
        /// <returns>Lista de marcas filtradas.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<BrandsForAdminDTO> GetFilteredForAdminAsync(BrandSearchTermDTO searchParams)
        {
            if (searchParams.PageNumber < 1)
            {
                throw new ArgumentOutOfRangeException("El número de página debe ser mayor o igual a 1.");
            }
            if (searchParams.PageSize < 1)
            {
                throw new ArgumentOutOfRangeException("El tamaño de página debe ser mayor o igual a 1.");
            }
            Log.Information("Obteniendo marcas filtradas para admin.");
            var (brands, totalCount) = await _brandRepository.GetFilteredForAdminAsync(searchParams);

            var totalPages = (int)
                Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
            int currentPage = searchParams.PageNumber;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            if (currentPage < 1 || currentPage > totalPages)
            {
                throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");
            }
            Log.Information("Total de categorias encontrados: {TotalCount}", totalCount);

            return new BrandsForAdminDTO
            {
                Brands = brands.Adapt<List<BrandForAdminDTO>>(),
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<BrandDetailDTO> GetByIdForAdminAsync(int id)
        {
            Log.Information("Obteniendo marca por ID para admin. ID: {BrandId}", id);
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                throw new KeyNotFoundException("Marca no encontrada.");
            }


            var productCount = await _productRepository.CountProductsByBrandIdAsync(id);

            Log.Information("Número de productos en la marca ID {BrandId}: {ProductCount}", id, productCount);

            var brandDetail = new BrandDetailDTO
            {
                Id = brand.Id,
                Name = brand.Name,
                CreatedAt = brand.CreatedAt,
                ProductCount = productCount
            };
            Log.Information("Marca encontrada. ID: {BrandId}", id);
            return brandDetail;
        }
    }
}