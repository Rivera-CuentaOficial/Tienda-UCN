using Microsoft.EntityFrameworkCore;
using Serilog;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class BlacklistedTokensRepository : IBlacklistedTokensRepository
    {
        private readonly DataContext _context;

        public BlacklistedTokensRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> AddTokenAsync(BlacklistedToken token)
        {
            Log.Information("Bloqueando token: {Token}", token.Token);
            var tokenExists = await _context.BlacklistedTokens
                .AnyAsync(t => t.Token == token.Token);
            if (tokenExists)
            {
                Log.Warning("El token {Token} ya está bloqueado", token.Token);
                return false;
            }
            try
            {
                await _context.BlacklistedTokens.AddAsync(token);
                await _context.SaveChangesAsync();
                Log.Information("Token bloqueado: {Token}", token.Token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al bloquear el token para el usuario: {UserId}", token.UserId);
                return false;
            }
            return true;
        }

        public async Task<bool> IsBlacklistedAsync(string token)
        {
            var exists = await _context.BlacklistedTokens
                .AnyAsync(t => t.Token == token && t.ExpiresAt > DateTime.UtcNow);
            return exists;
        }

        public async Task<int> CleanExpiredTokensAsync()
        {
            var expiredTokens = await _context.BlacklistedTokens
                .Where(t => t.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();
            _context.BlacklistedTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
            Log.Information("Eliminando {Count} tokens expirados", expiredTokens.Count);

            return expiredTokens.Count;
        }
    }
}