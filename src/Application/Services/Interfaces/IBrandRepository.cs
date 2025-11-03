using TiendaUCN.src.Application.DTOs.BrandResponse.AdminDTO;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IBrandService
    {
        Task<BrandsForAdminDTO> GetFilteredForAdminAsync(BrandSearchTermDTO brandSearchTermDTO);
        Task<BrandDetailDTO> GetByIdForAdminAsync(int id);
    }
}