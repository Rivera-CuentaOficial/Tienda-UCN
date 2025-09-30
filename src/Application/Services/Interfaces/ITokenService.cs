using TiendaUCN.Domain.Models;

namespace TiendaUCN.Application.Services.Interfaces;

public interface ITokenService
{
    string CreateToken(User user, string roleName, bool rememberMe);
}