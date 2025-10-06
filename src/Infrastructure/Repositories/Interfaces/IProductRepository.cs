using TiendaUCN.src.Application.DTOs.ProductResponse;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces;

public interface IProductRepository
{
    Task<int> CreateAsync(Product product);
    Task<Brand> CreateOrGetBrandAsync(string brandName);
    Task<Category> CreateOrGetCategoryAsync(string categoryName);
    Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForAdminAsync(
        SearchParamsDTO searchParams
    );
    Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForCustomerAsync(
        SearchParamsDTO searchParams
    );
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByIdForAdminAsync(int id);
    Task ToggleActiveAsync(int id);
}