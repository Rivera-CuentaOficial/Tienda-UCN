namespace TiendaUCN.src.Domain.Models;

public class Image
{
    public int Id { get; set; }
    public required string ImageUrl { get; set; }
    public required string PublicId { get; set; }
    public int ProductId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}