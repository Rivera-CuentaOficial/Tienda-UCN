using Mapster;

namespace TiendaUCN.Application.Mappers;

public static class MapperExtensions
{
    public static void ConfigureMapster()
    {
        UserMapper.ConfigureAllMapping();
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
    }
}