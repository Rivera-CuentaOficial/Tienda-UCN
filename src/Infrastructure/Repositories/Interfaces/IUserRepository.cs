using TiendaUCN.Domain.Models;

namespace TiendaUCN.Application.Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User> GetByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<string> GetUserRoleAsync(User user);
}