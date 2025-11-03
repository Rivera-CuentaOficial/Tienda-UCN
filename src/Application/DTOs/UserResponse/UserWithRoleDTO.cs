using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.DTOs.UserResponse
{
    public class UserWithRoleDTO
    {
        public User User { get; set; } = null!;
        public string RoleName { get; set; } = null!;
    }
}