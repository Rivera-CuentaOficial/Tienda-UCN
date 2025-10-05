namespace TiendaUCN.Application.DTOs.ProductResponse.Admin;

public class ListedProductsForAdminDTO
{
    public List<ProductForAdminDTO> Products { get; set; } = new List<ProductForAdminDTO>();

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }
}