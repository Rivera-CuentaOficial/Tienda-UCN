using TiendaUCN.src.Application.DTOs.BrandResponse.AdminDTO;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    public interface IBrandRepository
    {
        Task<(IEnumerable<Brand> brands, int totalCount)> GetFilteredForAdminAsync(BrandSearchTermDTO searchParams);
        Task<Brand?> GetByIdAsync(int id);
        Task<Brand> CreateOrGetBrandAsync(string brandName);
    }
}