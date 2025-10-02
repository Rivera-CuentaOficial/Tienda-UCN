using TiendaUCN.Domain.Models;

namespace TiendaUCN.Application.Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User> GetByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<string> GetUserRoleAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByRutAsync(string rut);

    Task<bool> CreateAsync(User user, string password);
}