using Mapster;

namespace TiendaUCN.src.Application.Mappers;

public class MapperExtensions
{
    public static void ConfigureMapster(IServiceProvider serviceProvider)
    {
        var userMapper = serviceProvider.GetService<UserMapper>();
        userMapper?.ConfigureAllMappings();

        var productMapper = serviceProvider.GetService<ProductMapper>();
        productMapper?.ConfigureAllMappings();

        var cartMapper = serviceProvider.GetService<CartMapper>();
        cartMapper?.ConfigureAllMappings();

        var orderMapper = serviceProvider.GetService<OrderMapper>();
        orderMapper?.ConfigureAllMappings();

        var auditMapper = serviceProvider.GetService<AuditMapper>();
        auditMapper?.ConfigureAllMappings();

        var categoryAndBrandMapper = serviceProvider.GetService<CategoryAndBrandMapper>();
        categoryAndBrandMapper?.ConfigureAllMappings();

        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
    }
}