namespace TiendaUCN.src.Domain.Models
{
    public class BlacklistedToken
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public int UserId { get; set; }
        public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }

        public User User { get; set; } = null!;
    }
}