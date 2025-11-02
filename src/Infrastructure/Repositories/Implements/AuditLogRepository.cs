using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly DataContext _context;

        public AuditLogRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAsync(AuditLog auditLog)
        {
            await _context.AuditLogs.AddAsync(auditLog);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}