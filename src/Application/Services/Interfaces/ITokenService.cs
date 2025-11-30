using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Services.Interfaces;

public interface ITokenService
{
    string CreateToken(User user, string roleName, bool rememberMe);
    /*Task<bool> IsTokenBlacklisted(string token);
    Task<bool> AddTokenToBlacklist(string token, int userId, DateTime expiresAt);*/
}