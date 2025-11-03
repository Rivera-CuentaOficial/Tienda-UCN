using Mapster;
using Serilog;
using TiendaUCN.src.Application.DTOs.CategoryResponse.AdminDTO;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private int _defaultPageSize;

        public CategoryService(ICategoryRepository categoryRepository, IProductRepository productRepository, IConfiguration configuration)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _defaultPageSize =
                configuration.GetValue<int?>("Categories:DefaultPageSize")
                ?? throw new ArgumentNullException(
                    "La configuración para el tamaño de página por defecto de categorías no está establecida.");
        }

        /// <summary>
        /// Obtiene categorías filtradas para el panel de administración.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar categorías.</param>
        /// <returns>Lista de categorías filtradas.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<CategoriesForAdminDTO> GetFilteredForAdminAsync(CategorySearchTermDTO searchParams)
        {
            if (searchParams.PageNumber < 1)
            {
                throw new ArgumentOutOfRangeException("El número de página debe ser mayor o igual a 1.");
            }
            if (searchParams.PageSize < 1)
            {
                throw new ArgumentOutOfRangeException("El tamaño de página debe ser mayor o igual a 1.");
            }
            Log.Information("Obteniendo categorias filtradas para admin.");
            var (categories, totalCount) = await _categoryRepository.GetFilteredForAdminAsync(searchParams);

            var totalPages = (int)
                Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
            int currentPage = searchParams.PageNumber;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            if (currentPage < 1 || currentPage > totalPages)
            {
                throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");
            }
            Log.Information("Total de categorias encontrados: {TotalCount}", totalCount);

            return new CategoriesForAdminDTO
            {
                Categories = categories.Adapt<List<CategoryForAdminDTO>>(),
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<CategoryDetailDTO> GetByIdForAdminAsync(int id)
        {
            Log.Information("Obteniendo categoría por ID para admin. ID: {CategoryId}", id);
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException("Categoría no encontrada.");
            }


            var productCount = await _productRepository.CountProductsByCategoryIdAsync(id);

            Log.Information("Número de productos en la categoría ID {CategoryId}: {ProductCount}", id, productCount);

            var categoryDetail = new CategoryDetailDTO
            {
                Id = category.Id,
                Name = category.Name,
                CreatedAt = category.CreatedAt,
                ProductCount = productCount
            };
            Log.Information("Categoría encontrada. ID: {CategoryId}", id);
            return categoryDetail;
        }
    }
}