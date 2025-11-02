using Mapster;
using TiendaUCN.src.Application.DTOs.AuthResponse;
using TiendaUCN.src.Application.DTOs.UserResponse;
using TiendaUCN.src.Application.DTOs.UserResponse.AdminDTO;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Mappers;

public class UserMapper
{
    public void ConfigureAllMapping()
    {
        ConfigureAuthMapping();
        ConfigureProfileMapping();
        ConfigureUpdateProfileMapping();
        ConfigureUsersMapping();
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
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
            .Map(dest => dest.LastLoginAt, src => src.LastLoginAt);
    }

    public static void ConfigureUpdateProfileMapping()
    {
        TypeAdapterConfig<UpdateProfileDTO, User>
            .NewConfig()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.Email!)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.Gender, src => src.Gender)
            .Map(dest => dest.BirthDate, src => src.BirthDate)
            .Map(dest => dest.Rut, src => src.Rut)
            .Map(dest => dest.PendingEmail, src => src.Email)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber);
    }

    public static void ConfigureUsersMapping()
    {
        TypeAdapterConfig<UserWithRoleDTO, UserForAdminDTO>
            .NewConfig()
            .Map(dest => dest.Id, src => src.User.Id)
            .Map(dest => dest.FullName, src => $"{src.User.FirstName} {src.User.LastName}")
            .Map(dest => dest.Email, src => src.User.Email)
            .Map(dest => dest.RoleName, src => src.RoleName)
            .Map(dest => dest.CreatedAt, src => src.User.RegisteredAt)
            .Map(dest => dest.LastLoginAt, src => src.User.LastLoginAt)
            .Map(dest => dest.Status, src => src.User.Status);

        TypeAdapterConfig<User, UserDetailsForAdminDTO>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}")
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.CreatedAt, src => src.RegisteredAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt)
            .Map(dest => dest.LastLoginAt, src => src.LastLoginAt)
            .Map(dest => dest.Status, src => src.Status);
    }
}