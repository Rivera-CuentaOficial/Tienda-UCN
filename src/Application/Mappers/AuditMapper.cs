using Mapster;
using TiendaUCN.src.Application.DTOs.UserResponse.AdminDTO;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Mappers
{
    public class AuditMapper
    {
        public void ConfigureAllMappings()
        {
            ConfigureAuditMapping();
        }

        public static void ConfigureAuditMapping()
        {
            TypeAdapterConfig<UpdateUserStatusDTO, AuditLog>
                .NewConfig()
                .IgnoreNullValues(true)
                .Map(dest => dest.NewStatus, src => src.Status)
                .Map(dest => dest.Reason, src => src.Reason);
        }
    }
}