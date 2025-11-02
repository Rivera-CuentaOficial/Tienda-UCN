namespace TiendaUCN.src.Application.DTOs.UserResponse.AdminDTO
{
    public class UserForAdminDTO
    {
        public required int Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string RoleName { get; set; }
        public required string Status { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime LastLoginAt { get; set; }
    }
}