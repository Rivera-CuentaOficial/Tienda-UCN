namespace TiendaUCN.src.Application.DTOs.ProductResponse.AdminDTO
{
    public class ProductDetailForAdminDTO
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Price { get; set; }
        public required int Discount { get; set; }
        public required int Stock { get; set; }
        public required bool IsAvailable { get; set; }
        public required string CategoryName { get; set; }
        public required string BrandName { get; set; }
        public required string Status { get; set; }
        public required bool IsDeleted { get; set; }
        public List<string> ImagesURL { get; set; } = new List<string>();
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}