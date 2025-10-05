namespace TiendaUCN.Domain.Models;

public class Brand
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
}