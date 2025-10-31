using Mapster;
using TiendaUCN.src.Application.DTOs.AuthResponse;
using TiendaUCN.src.Application.DTOs.UserResponse;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Mappers;

public class UserMapper
{
    public void ConfigureAllMapping()
    {
        ConfigureAuthMapping();
        ConfigureProfileMapping();
    }

    public static void ConfigureAuthMapping()
    {
        TypeAdapterConfig<RegisterDTO, User>
            .NewConfig()
            .Map(dest => dest.UserName, src => src.Email)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Rut, src => src.Rut)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.BirthDate, src => src.BirthDate)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
            .Map(dest => dest.Gender, src => src.Gender)
            .Map(dest => dest.EmailConfirmed, src => false);
    }

    public static void ConfigureProfileMapping()
    {
        TypeAdapterConfig<User, ViewUserProfileDTO>
            .NewConfig()
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.Gender, src => src.Gender)
            .Map(dest => dest.BirthDate, src => src.BirthDate)
            .Map(dest => dest.Rut, src => src.Rut)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber);
    }
}