using TiendaUCN.src.Application.DTOs.CategoryResponse.AdminDTO;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<(IEnumerable<Category> categories, int totalCount)> GetFilteredForAdminAsync(CategorySearchTermDTO searchParams);
        Task<Category?> GetByIdAsync(int id);
        Task<Category> CreateOrGetCategoryAsync(string categoryName);
    }
}