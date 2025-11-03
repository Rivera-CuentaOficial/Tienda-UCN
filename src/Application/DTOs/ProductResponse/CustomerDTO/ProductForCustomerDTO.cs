namespace TiendaUCN.src.Application.DTOs.ProductResponse.CustomerDTO;

public class ProductForCustomerDTO
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string MainImageURL { get; set; }
    public required string Price { get; set; }
    public required int Discount { get; set; }
}