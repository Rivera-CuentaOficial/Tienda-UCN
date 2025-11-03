namespace TiendaUCN.src.Application.DTOs.BrandResponse.AdminDTO
{
    public class BrandDetailDTO
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required int ProductCount { get; set; }
    }
}