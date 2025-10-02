using Mapster;
using Serilog;
using TiendaUCN.Application.DTOs.AuthResponse;
using TiendaUCN.Application.Infrastructure.Repositories.Interfaces;
using TiendaUCN.Application.Services.Interfaces;
using TiendaUCN.Domain.Models;
using TiendaUCN.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.Application.Services.Implements;

public class UserService : IUserService
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IVerificationCodeRepository _verificationCodeRepository;
    private readonly IConfiguration _configuration;
    private readonly int _verificationCodeExpirationInMinutes;

    public UserService(
        ITokenService tokenService,
        IUserRepository userRepository,
        IEmailService emailService,
        IVerificationCodeRepository verificationCodeRepository,
        IConfiguration configuration
    )
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
        _emailService = emailService;
        _verificationCodeRepository = verificationCodeRepository;
        _configuration = configuration;
        _verificationCodeExpirationInMinutes = _configuration.GetValue<int>(
            "VerificationCode:ExpirationTimeInMinutes"
        );
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

    public async Task<string> RegisterAsync(RegisterDTO registerDTO, HttpContext httpContext)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "IP desconocida";
        Log.Information(
            "Intento de registro para el correo {Email} desde la IP {IP}",
            registerDTO.Email,
            ipAddress
        );

        bool isRegistered = await _userRepository.ExistsByEmailAsync(registerDTO.Email);
        if (isRegistered)
        {
            Log.Warning(
                "Intento de registro fallido para el correo {Email} desde la IP {IP} - Email ya registrado",
                registerDTO.Email,
                ipAddress
            );
            throw new InvalidOperationException("El correo electrónico ya está registrado.");
        }

        isRegistered = await _userRepository.ExistsByRutAsync(registerDTO.Rut);
        if (isRegistered)
        {
            Log.Warning(
                "Intento de registro fallido para el Rut {Rut} desde la IP {IP} - Rut ya registrado",
                registerDTO.Rut,
                ipAddress
            );
            throw new InvalidOperationException("El Rut ya está registrado.");
        }
        var user = registerDTO.Adapt<User>();
        var result = await _userRepository.CreateAsync(user, registerDTO.Password);
        if (!result)
        {
            Log.Error(
                "Error al registrar el usuario con correo {Email} desde la IP {IP}",
                registerDTO.Email,
                ipAddress
            );
            throw new Exception("Error al registrar el usuario.");
        }
        Log.Information(
            "Registro exitoso para el usuario {Email} desde la IP {IP}",
            registerDTO.Email,
            ipAddress
        );
        var code = new Random().Next(100000, 999999).ToString();
        VerificationCode verificationCode = new VerificationCode
        {
            Type = CodeType.EmailVerification,
            Code = code,
            Expiration = DateTime.UtcNow.AddMinutes(_verificationCodeExpirationInMinutes),
            UserId = user.Id,
        };
        var createNewVerificationCode = await _verificationCodeRepository.CreateAsync(
            verificationCode
        );
        Log.Information(
            "Código de verificación generado para el usuario {Email} desde la IP {IP}",
            registerDTO.Email,
            ipAddress
        );
        await _emailService.SendVerificationCodeEmailAsync(registerDTO.Email, code);
        Log.Information(
            "Correo de verificación enviado a {Email} desde la IP {IP}",
            registerDTO.Email,
            ipAddress
        );
        return "Usuario registrado exitosamente. Por favor, verifica tu correo electrónico.";
    }
}