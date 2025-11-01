using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    public interface IBlacklistedTokensRepository
    {
        Task<bool> AddTokenAsync(BlacklistedToken token);

        Task<bool> IsBlacklistedAsync(string token);

        Task<int> CleanExpiredTokensAsync();
    }
}