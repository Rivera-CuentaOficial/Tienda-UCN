using TiendaUCN.src.Application.DTOs.CategoryResponse.AdminDTO;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoriesForAdminDTO> GetFilteredForAdminAsync(CategorySearchTermDTO categorySearchTermDTO);
        Task<CategoryDetailDTO> GetByIdForAdminAsync(int id);
    }
}