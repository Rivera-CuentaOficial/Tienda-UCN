using TiendaUCN.src.Application.DTOs.ProductResponse;
using TiendaUCN.src.Application.DTOs.ProductResponse.AdminDTO;
using TiendaUCN.src.Application.DTOs.ProductResponse.CustomerDTO;

namespace TiendaUCN.src.Application.Services.Interfaces;

public interface IProductService
{
    Task<ListedProductsForAdminDTO> GetFilteredForAdminAsync(SearchParamsDTO searchParams);
    Task<ListedProductsForCustomerDTO> GetFilteredForCustomerAsync(SearchParamsDTO searchParams);
    Task<ProductDetailDTO> GetByIdAsync(int id);
    Task<ProductDetailDTO> GetByIdForAdminAsync(int id);
    Task<string> CreateAsync(CreateProductDTO createProductDTO);

    Task ToggleActiveAsync(int id);
}