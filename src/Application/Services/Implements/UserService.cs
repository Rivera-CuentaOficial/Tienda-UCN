using Mapster;
using Serilog;
using TiendaUCN.src.Application.DTOs.AuthResponse;
using TiendaUCN.src.Application.DTOs.UserResponse;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements;

public class UserService : IUserService
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IVerificationCodeRepository _verificationCodeRepository;
    private readonly IConfiguration _configuration;
    private readonly int _verificationCodeExpirationInMinutes;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="UserService"/>.
    /// </summary>
    /// <param name="tokenService">Servicio para la generación y validación de tokens.</param>
    /// <param name="userRepository">Repositorio para el acceso a datos de usuarios.</param>
    /// <param name="emailService">Servicio para el envío de correos electrónicos.</param>
    /// <param name="verificationCodeRepository">Repositorio para la gestión de códigos de verificación.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
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
        user.PhoneNumber = NormalizePhoneNumber(registerDTO.PhoneNumber);
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
        await _emailService.SendVerificationCodeEmailAsync(
            registerDTO.Email,
            createNewVerificationCode.Code
        );
        Log.Information(
            "Correo de verificación enviado a {Email} desde la IP {IP}",
            registerDTO.Email,
            ipAddress
        );
        return "Usuario registrado exitosamente. Por favor, verifica tu correo electrónico.";
    }

    public async Task<string> VerifyEmailAsync(VerifyDTO verifyDTO, HttpContext httpContext)
    {
        User? user = await _userRepository.GetByEmailAsync(verifyDTO.Email);
        if (user == null)
        {
            Log.Information(
                "Intento de verificación de correo fallido para {Email} - Usuario no encontrado",
                verifyDTO.Email
            );
            throw new KeyNotFoundException("Usuario no encontrado.");
        }
        if (user.EmailConfirmed)
        {
            Log.Information(
                "Intento de verificación de correo fallido para {Email} - Correo ya verificado",
                verifyDTO.Email
            );
            throw new InvalidOperationException("El correo ya ha sido verificado.");
        }
        CodeType codeType = CodeType.EmailVerification;

        var verificationCode = await _verificationCodeRepository.GetByLatestUserIdAsync(
            user.Id,
            codeType
        );
        if (verificationCode == null)
        {
            Log.Information(
                "Intento de verificación de correo fallido para {Email} - Código no encontrado",
                verifyDTO.Email
            );
            throw new KeyNotFoundException("Código de verificación no encontrado.");
        }
        if (
            verificationCode.Code != verifyDTO.VerificationCode
            || DateTime.UtcNow >= verificationCode.Expiration
        )
        {
            int attempsCountUpdated = await _verificationCodeRepository.IncreaseAttemptsAsync(
                user.Id,
                codeType
            );
            Log.Warning(
                $"Código de verificación incorrecto o expirado para el usuario: {verifyDTO.Email}. Intentos actuales: {attempsCountUpdated}"
            );
            if (attempsCountUpdated >= 5)
            {
                Log.Warning(
                    $"Se ha alcanzado el límite de intentos para el usuario: {verifyDTO.Email}"
                );
                bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(
                    user.Id,
                    codeType
                );
                if (codeDeleteResult)
                {
                    Log.Warning(
                        $"Se ha eliminado el código de verificación para el usuario: {verifyDTO.Email}"
                    );
                    bool userDeleteResult = await _userRepository.DeleteAsync(user.Id);
                    if (userDeleteResult)
                    {
                        Log.Warning($"Se ha eliminado el usuario: {verifyDTO.Email}");
                        throw new ArgumentException(
                            "Se ha alcanzado el límite de intentos. El usuario ha sido eliminado."
                        );
                    }
                }
            }
            if (DateTime.UtcNow >= verificationCode.Expiration)
            {
                Log.Warning(
                    $"El código de verificación ha expirado para el usuario: {verifyDTO.Email}"
                );
                throw new ArgumentException("El código de verificación ha expirado.");
            }
            else
            {
                Log.Warning(
                    $"El código de verificación es incorrecto para el usuario: {verifyDTO.Email}"
                );
                throw new ArgumentException(
                    $"El código de verificación es incorrecto, quedan {5 - attempsCountUpdated} intentos."
                );
            }
        }
        bool emailConfirmed = await _userRepository.ConfirmEmailAsync(user.Email!);
        if (emailConfirmed)
        {
            bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(
                user.Id,
                codeType
            );
            if (codeDeleteResult)
            {
                Log.Warning(
                    $"Se ha eliminado el código de verificación para el usuario: {verifyDTO.Email}"
                );
                await _emailService.SendWelcomeEmailAsync(user.Email!);
                Log.Information(
                    $"El correo electrónico del usuario {verifyDTO.Email} ha sido confirmado exitosamente."
                );
                return "!Ya puedes iniciar sesión y disfrutar de todos los beneficios de Tienda UCN!";
            }
            throw new Exception("Error al confirmar el correo electrónico.");
        }
        throw new Exception("Error al verificar el correo electrónico.");
    }

    public async Task<string> ResendVerifyEmail(ResendVerifyDTO resendVerifyDTO)
    {
        var currentTime = DateTime.UtcNow;
        User? user = await _userRepository.GetByEmailAsync(resendVerifyDTO.Email);
        if (user == null)
        {
            Log.Warning($"El usuario con el correo {resendVerifyDTO.Email} no existe.");
            throw new KeyNotFoundException("El usuario no existe.");
        }
        if (user.EmailConfirmed)
        {
            Log.Warning(
                $"El usuario con el correo {resendVerifyDTO.Email} ya ha verificado su correo electrónico."
            );
            throw new InvalidOperationException("El correo electrónico ya ha sido verificado.");
        }
        VerificationCode? verificationCode =
            await _verificationCodeRepository.GetLatestByUserIdAsync(
                user.Id,
                CodeType.EmailVerification
            );
        var expirationTime = verificationCode!.CreatedAt.AddMinutes(
            _verificationCodeExpirationInMinutes
        );
        if (expirationTime > currentTime)
        {
            int remainingSeconds = (int)(expirationTime - currentTime).TotalSeconds;
            Log.Warning(
                $"El usuario {resendVerifyDTO.Email} ha solicitado un reenvío del código de verificación antes de los {_verificationCodeExpirationInMinutes} minutos."
            );
            throw new TimeoutException(
                $"Debe esperar {remainingSeconds} segundos para solicitar un nuevo código de verificación."
            );
        }
        string newCode = new Random().Next(100000, 999999).ToString();
        verificationCode.Code = newCode;
        verificationCode.Expiration = DateTime.UtcNow.AddMinutes(
            _verificationCodeExpirationInMinutes
        );
        await _verificationCodeRepository.UpdateAsync(verificationCode);
        Log.Information(
            $"Nuevo código de verificación generado para el usuario: {resendVerifyDTO.Email} - Código: {newCode}"
        );
        await _emailService.SendVerificationCodeEmailAsync(user.Email!, newCode);
        Log.Information(
            $"Se ha reenviado un nuevo código de verificación al correo electrónico: {resendVerifyDTO.Email}"
        );
        return "Se ha reenviado un nuevo código de verificación a su correo electrónico.";
    }

    public async Task<string> SendPasswordRecoveryEmail(
        RecoverPasswordDTO recoverPasswordDTO,
        HttpContext httpContext
    )
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "IP desconocida";
        User? user = await _userRepository.GetByEmailAsync(recoverPasswordDTO.Email);
        if (user == null)
        {
            Log.Warning($"El usuario con el correo {recoverPasswordDTO.Email} no existe.");
            throw new KeyNotFoundException("El usuario no existe.");
        }
        if (!user.EmailConfirmed)
        {
            Log.Warning(
                $"EL usuario con el correo {recoverPasswordDTO} no ha verificado su correo."
            );
            throw new InvalidOperationException("El usuario no ha activado su cuenta.");
        }
        var code = new Random().Next(100000, 999999).ToString();
        VerificationCode verificationCode = new VerificationCode
        {
            Type = CodeType.PasswordReset,
            Code = code,
            Expiration = DateTime.UtcNow.AddMinutes(_verificationCodeExpirationInMinutes),
            UserId = user.Id,
        };
        var createNewVerificationCode = await _verificationCodeRepository.CreateAsync(
            verificationCode
        );
        Log.Information(
            "Código de para cambio de contraseña generado para el usuario {Email} desde la IP {IP}",
            recoverPasswordDTO.Email,
            ipAddress
        );
        await _emailService.SendPasswordRecoveryEmail(
            recoverPasswordDTO.Email,
            createNewVerificationCode.Code
        );
        Log.Information(
            "Correo de verificación enviado a {Email} desde la IP {IP}",
            recoverPasswordDTO.Email,
            ipAddress
        );
        return "Codigo de recuperacion enviado exitosamente.";
    }

    public async Task<string> ChangeUserPasswordByEmailAsync(ResetPasswordDTO resetPasswordDTO)
    {
        User? user = await _userRepository.GetByEmailAsync(resetPasswordDTO.Email);
        if (user == null)
        {
            Log.Information(
                "Intento de cambio de contraseña fallido para {Email} - Usuario no encontrado",
                resetPasswordDTO.Email
            );
            throw new KeyNotFoundException("Usuario no encontrado.");
        }
        if (!user.EmailConfirmed)
        {
            Log.Information(
                "Intento de cambio de contraseña fallido para {Email} - Correo no verificado",
                resetPasswordDTO.Email
            );
            throw new InvalidOperationException("El correo no ha sido verificado.");
        }

        CodeType type = CodeType.PasswordReset;

        var verificationCode = await _verificationCodeRepository.GetByLatestUserIdAsync(
            user.Id,
            type
        );
        if (verificationCode == null)
        {
            Log.Information(
                "Intento de cambio de contraseña fallido para {Email} - Código no encontrado",
                resetPasswordDTO.Email
            );
            throw new KeyNotFoundException("Código de cambio de contraseña no encontrado.");
        }
        if (
            verificationCode.Code != resetPasswordDTO.Code
            || DateTime.UtcNow >= verificationCode.Expiration
        )
        {
            int attempsCountUpdated = await _verificationCodeRepository.IncreaseAttemptsAsync(
                user.Id,
                type
            );
            Log.Warning(
                $"Código de cambio de contraseña incorrecto o expirado para el usuario: {resetPasswordDTO.Email}. Intentos actuales: {attempsCountUpdated}"
            );
            if (attempsCountUpdated >= 5)
            {
                Log.Warning(
                    $"Se ha alcanzado el límite de intentos para el usuario: {resetPasswordDTO.Email}"
                );
                bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(
                    user.Id,
                    type
                );
                if (codeDeleteResult)
                {
                    Log.Warning(
                        $"Se ha eliminado el código de cambio de contraseña para el usuario: {resetPasswordDTO.Email}"
                    );
                }
            }
            if (DateTime.UtcNow >= verificationCode.Expiration)
            {
                Log.Warning(
                    $"El código de cambio de contraseña ha expirado para el usuario: {resetPasswordDTO.Email}"
                );
                throw new ArgumentException("El código de cambio de contraseña ha expirado.");
            }
            else
            {
                Log.Warning(
                    $"El código de cambio de contraseña es incorrecto para el usuario: {resetPasswordDTO.Email}"
                );
                throw new ArgumentException(
                    $"El código de cambio de contraseña es incorrecto, quedan {5 - attempsCountUpdated} intentos."
                );
            }
        }
        bool changePassword = await _userRepository.ChangeUserPasswordAsync(
            user,
            resetPasswordDTO.NewPassword
        );
        if (!changePassword)
        {
            Log.Warning(
                $"No se pudo actualizar la contraseña para el usuario {resetPasswordDTO.Email}."
            );
            throw new InvalidOperationException("No se pudo actualizar la contraseña.");
        }
        Log.Information($"Contraseña actualizada para el usuario {resetPasswordDTO.Email}");
        await _verificationCodeRepository.DeleteByUserIdAsync(user.Id, type);
        Log.Warning(
            $"Se ha eliminado el código de cambio de contraseña para el usuario: {resetPasswordDTO.Email}"
        );
        return "Se ha reseteado la contraseña correctamente";
    }

    /// <summary>
    /// Normaliza un número de teléfono al formato internacional chileno.
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <returns>El número de teléfono normalizado.</returns>
    public string NormalizePhoneNumber(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return "+56 " + digits;
    }

    /// <summary>
    /// Elimina los usuarios no confirmados.
    /// </summary>
    /// <returns>El número de usuarios eliminados.</returns>
    public async Task<int> DeleteUnconfirmedAsync()
    {
        return await _userRepository.DeleteUnconfirmedAsync();
    }

    /// <summary>
    /// Obtiene el perfil de un usuario por su ID.
    /// </summary>
    /// <param name="userId">Id del usuario</param>
    /// <returns>Datos del perfil del usuario</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<ViewUserProfileDTO> GetUserProfileAsync(int userId)
    {
        Log.Information("Obteniendo perfil de usuario para el usuario ID: {UserId}", userId);
        User? user =
            await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");
        Log.Information("Perfil de usuario obtenido para el usuario ID: {UserId}", userId);
        return user.Adapt<ViewUserProfileDTO>();
    }
}