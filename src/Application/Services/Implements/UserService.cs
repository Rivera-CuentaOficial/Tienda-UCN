using Serilog;
using TiendaUCN.Application.DTOs.AuthResponse;
using TiendaUCN.Application.Infrastructure.Repositories.Interfaces;
using TiendaUCN.Application.Services.Interfaces;

namespace TiendaUCN.Application.Services.Implements;

public class UserService : IUserService
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;

    public UserService(ITokenService tokenService, IUserRepository userRepository)
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
    }

    public async Task<string> LoginAsync(LoginDTO loginDTO, HttpContext httpContext)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "IP desconocida";
        var user = await _userRepository.GetByEmailAsync(loginDTO.Email);
        // Verificar si el usuario existe
        if (user == null)
        {
            Log.Warning(
                "Intento de inicio de sesión fallido para el correo {Email} desde la IP {IP}",
                loginDTO.Email,
                ipAddress
            );
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }
        if (!user.EmailConfirmed)
        {
            Log.Warning(
                "Intento de inicio de sesión fallido para el correo {Email} desde la IP {IP} - Email no confirmado",
                loginDTO.Email,
                ipAddress
            );
            throw new InvalidOperationException("El correo electrónico no ha sido confirmado.");
        }
        // Verificar la contraseña
        var result = await _userRepository.CheckPasswordAsync(user, loginDTO.Password);
        if (!result)
        {
            Log.Warning(
                "Intento de inicio de sesión fallido para el correo {Email} desde la IP {IP} - Contraseña incorrecta",
                loginDTO.Email,
                ipAddress
            );
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }
        // Obtener el rol del usuario
        var roleName = await _userRepository.GetUserRoleAsync(user);
        // Generar el token JWT
        Log.Information(
            "Inicio de sesión exitoso para el correo {Email} desde la IP {IP}",
            loginDTO.Email,
            ipAddress
        );
        return _tokenService.CreateToken(user, roleName, loginDTO.RememberMe);
    }
}