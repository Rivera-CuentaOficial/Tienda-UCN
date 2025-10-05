using TiendaUCN.Application.DTOs.ProductResponse;
using TiendaUCN.Application.DTOs.ProductResponse.Admin;
using TiendaUCN.Application.DTOs.ProductResponse.Customer;

namespace TiendaUCN.Application.Services.Interfaces;

public interface IProductService
{
    Task<ListedProductsForAdminDTO> GetFilteredForAdminAsync(SearchParamsDTO searchParams);
    Task<ListedProductsForCustomerDTO> GetFilteredForCustomerAsync(SearchParamsDTO searchParams);
    Task<ProductDetailDTO> GetByIdAsync(int id);
    Task<ProductDetailDTO> GetByIdForAdminAsync(int id);
    Task<string> CreateAsync(CreateProductDTO createProductDTO);

    Task ToggleActiveAsync(int id);
}