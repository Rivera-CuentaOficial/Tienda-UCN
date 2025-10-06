using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User> GetByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<string> GetUserRoleAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByRutAsync(string rut);
    Task<bool> CreateAsync(User user, string password);
    Task<bool> ConfirmEmailAsync(string email);
    Task<bool> ChangeUserPasswordAsync(User user, string newPassword);
    Task<bool> DeleteAsync(int userId);
    Task<int> DeleteUnconfirmedAsync();

}