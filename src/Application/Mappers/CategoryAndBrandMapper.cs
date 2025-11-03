using Mapster;
using TiendaUCN.src.Application.DTOs.CategoryResponse.AdminDTO;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Mappers
{
    public class CategoryAndBrandMapper
    {
        public void ConfigureAllMappings()
        {
            ConfigureCategoryMappings();
            //ConfigureBrandMappings();
        }

        public void ConfigureCategoryMappings()
        {
            TypeAdapterConfig<Category, CategoryForAdminDTO>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name);
            TypeAdapterConfig<Category, CategoryDetailDTO>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Ignore(dest => dest.ProductCount);
        }
    }
}