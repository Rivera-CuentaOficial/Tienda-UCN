using Mapster;

namespace TiendaUCN.Application.Mappers;

public class MapperExtensions
{
    public static void ConfigureMapster(IServiceProvider serviceProvider)
    {
        var userMapper = serviceProvider.GetService<UserMapper>();
        userMapper?.ConfigureAllMapping();

        var productMapper = serviceProvider.GetService<ProductMapper>();
        productMapper?.ConfigureAllMappings();

        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
    }
}