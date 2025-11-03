namespace TiendaUCN.src.Application.DTOs.CategoryResponse.AdminDTO
{
    public class CategoryDetailDTO
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required int ProductCount { get; set; }
    }
}