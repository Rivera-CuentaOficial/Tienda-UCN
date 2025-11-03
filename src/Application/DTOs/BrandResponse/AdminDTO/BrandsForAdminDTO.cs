namespace TiendaUCN.src.Application.DTOs.BrandResponse.AdminDTO
{
    public class BrandsForAdminDTO
    {
        public List<BrandForAdminDTO> Brands { get; set; } = new List<BrandForAdminDTO>();

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }
    }
}