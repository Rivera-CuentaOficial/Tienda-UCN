using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<bool> CreateAsync(AuditLog auditLog);
    }
}