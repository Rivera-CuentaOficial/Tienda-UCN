namespace TiendaUCN.Application.DTOs.ProductResponse.Customer;

public class ProductDetailDTO
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public List<string> ImagesURL { get; set; } = new List<string>();
    public required string Price { get; set; }
    public required int Discount { get; set; }
    public required int Stock { get; set; }
    public required string StockIndicator { get; set; }
    public required string CategoryName { get; set; }
    public required string BrandName { get; set; }
    public required string StatusName { get; set; }
    public required bool IsAvailable { get; set; }
}