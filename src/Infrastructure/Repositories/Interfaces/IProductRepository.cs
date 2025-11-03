using TiendaUCN.src.Application.DTOs.ProductResponse;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces;

public interface IProductRepository
{
    Task<int> CreateAsync(Product product);
    Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForAdminAsync(
        SearchParamsDTO searchParams
    );
    Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForCustomerAsync(
        SearchParamsDTO searchParams
    );
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByIdForAdminAsync(int id);
    Task ToggleActiveAsync(int id);
    Task<int> GetRealStockAsync(int productId);
    Task UpdateStockAsync(int productId, int stock);
    Task<int> CountProductsByCategoryIdAsync(int categoryId);
    Task<int> CountProductsByBrandIdAsync(int brandId);
}