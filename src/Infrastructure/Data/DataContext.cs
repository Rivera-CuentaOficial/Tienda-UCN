using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Data;

public class DataContext : IdentityDbContext<User, Role, int>
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }

    public DbSet<VerificationCode> VerificationCodes { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
}