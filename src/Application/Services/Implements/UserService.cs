using Bogus.DataSets;
using Mapster;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using TiendaUCN.src.Application.DTOs.AuthResponse;
using TiendaUCN.src.Application.DTOs.UserResponse;
using TiendaUCN.src.Application.DTOs.UserResponse.AdminDTO;
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
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IConfiguration _configuration;
    private readonly int _verificationCodeExpirationInMinutes;
    private readonly int _defaultPageSize;

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
        IAuditLogRepository auditLogRepository,
        IConfiguration configuration
    )
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
        _emailService = emailService;
        _verificationCodeRepository = verificationCodeRepository;
        _auditLogRepository = auditLogRepository;
        _configuration = configuration;
        _verificationCodeExpirationInMinutes = _configuration.GetValue<int>(
            "VerificationCode:ExpirationTimeInMinutes"
        );
        _defaultPageSize = _configuration.GetValue<int>("Users:DefaultPageSize");
    }

    /// <summary>
    /// Inicia sesión de un usuario y genera un token JWT.
    /// </summary>
    /// <param name="loginDTO">Datos de inicio de sesión del usuario.</param>
    /// <param name="httpContext">Contexto HTTP de la solicitud.</param>
    /// <returns>Token JWT del usuario.</returns>
    /// <exception cref="UnauthorizedAccessException">Credenciales inválidas.</exception>
    /// <exception cref="InvalidOperationException">El correo electrónico no ha sido confirmado.</exception>
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
        user.LastLoginAt = DateTime.UtcNow;
        // Generar el token JWT
        Log.Information(
            "Inicio de sesión exitoso para el correo {Email} desde la IP {IP}",
            loginDTO.Email,
            ipAddress
        );
        return _tokenService.CreateToken(user, roleName, loginDTO.RememberMe);
    }

    /// <summary>
    /// Registra un nuevo usuario.
    /// </summary>
    /// <param name="registerDTO">Datos de registro del nuevo usuario.</param>
    /// <param name="httpContext">Contexto HTTP de la solicitud.</param>
    /// <returns>Mensaje de éxito o error.</returns>
    /// <exception cref="InvalidOperationException">El correo electrónico o RUT ya están registrados.</exception>
    /// <exception cref="Exception">Error al registrar el usuario.</exception>
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

    /// <summary>
    /// Verifica el correo electrónico de un usuario.
    /// </summary>
    /// <param name="verifyDTO">Datos de verificación del usuario.</param>
    /// <param name="httpContext">Contexto HTTP de la solicitud.</param>
    /// <returns>Mensaje de éxito o error.</returns>
    /// <exception cref="KeyNotFoundException">El usuario no fue encontrado.</exception>
    /// <exception cref="InvalidOperationException">El correo electrónico no ha sido confirmado.</exception>
    /// <exception cref="ArgumentException">El código de verificación es inválido.</exception>
    /// <exception cref="Exception">Error al verificar el correo electrónico.</exception>
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

    /// <summary>
    /// Reenvía el código de verificación al correo electrónico del usuario.
    /// </summary>
    /// <param name="resendVerifyDTO">Datos del usuario para reenviar el código.</param>
    /// <returns>Mensaje de éxito o error.</returns>
    /// <exception cref="KeyNotFoundException">El usuario no existe.</exception>
    /// <exception cref="InvalidOperationException">El correo electrónico ya ha sido verificado.</exception>
    /// <exception cref="TimeoutException">Ha pedido un nuevo código de verificación muchas veces en un corto período.</exception>
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

    /// <summary>
    /// Envía un correo de recuperación de contraseña al usuario.
    /// </summary>
    /// <param name="recoverPasswordDTO">Datos del usuario para recuperar la contraseña.</param>
    /// <param name="httpContext">Contexto HTTP de la solicitud.</param>
    /// <returns>Mensaje de éxito o error.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
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

    /// <summary>
    /// Cambia la contraseña de un usuario mediante su correo electrónico.
    /// </summary>
    /// <param name="resetPasswordDTO">Datos del usuario para restablecer la contraseña.</param>
    /// <returns>Mensaje de éxito o error.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
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
    /// Cambia la contraseña de un usuario.
    /// </summary>
    /// <param name="token">Token de autenticacion</param>
    /// <param name="userId">ID del usuario</param>
    /// <param name="expiration">Fecha de expiración del token</param>
    /// <param name="changePasswordDTO">Datos de la nueva contraseña</param>
    /// <returns>Mensaje de éxito</returns>
    public async Task<string> ChangeUserPasswordAsync(
        string token,
        int userId,
        DateTime expiration,
        ChangePasswordDTO changePasswordDTO
    )
    {
        User user =
            await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");
        var isPsswordValid = await _userRepository.CheckPasswordAsync(
            user,
            changePasswordDTO.CurrentPassword
        );
        if (!isPsswordValid)
        {
            Log.Warning($"La contraseña actual es incorrecta para el usuario ID: {userId}");
            throw new InvalidOperationException("La contraseña actual es incorrecta.");
        }
        bool changePassword = await _userRepository.ChangeUserPasswordAsync(
            user,
            changePasswordDTO.NewPassword
        );
        if (!changePassword)
        {
            Log.Warning($"No se pudo actualizar la contraseña para el usuario ID: {userId}");
            throw new InvalidOperationException("No se pudo actualizar la contraseña.");
        }
        Log.Information($"Contraseña actualizada para el usuario ID: {userId}", userId);
        var blacklistToken = await _tokenService.AddTokenToBlacklist(token, userId, expiration);
        if (!blacklistToken)
        {
            Log.Warning($"No se pudieron invalidar los tokens para el usuario ID: {userId}");
            throw new InvalidOperationException("No se pudieron invalidar los tokens.");
        }
        return "Contraseña cambiada exitosamente.";
    }

    /// <summary>
    /// Normaliza un número de teléfono al formato internacional chileno.
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <returns>El número de teléfono normalizado.</returns>
    public string NormalizePhoneNumber(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return "+569 " + digits;
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

    /// <summary>
    /// Actualiza el perfil de un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario a actualizar</param>
    /// <param name="updateProfileDTO">Datos a actualizar</param>
    /// <returns>Mensaje de éxito</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<string> UpdateUserProfileAsync(int userId, UpdateProfileDTO updateProfileDTO)
    {
        Log.Information("Actualizando perfil de usuario para el usuario ID: {UserId}", userId);
        User? user =
            await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");
        if (updateProfileDTO.Email != null)
        {
            if (updateProfileDTO.Email == user.Email)
            {
                throw new InvalidOperationException(
                    "El nuevo correo electrónico no puede ser igual al actual."
                );
            }
            bool emailExists = await _userRepository.ExistsByEmailAsync(updateProfileDTO.Email);
            if (emailExists)
            {
                throw new InvalidOperationException("El correo electrónico ya está en uso.");
            }
            string verificationCode = new Random().Next(100000, 999999).ToString();
            VerificationCode code = new VerificationCode
            {
                Type = CodeType.EmailVerification,
                Code = verificationCode,
                Expiration = DateTime.UtcNow.AddMinutes(_verificationCodeExpirationInMinutes),
                UserId = user.Id,
            };
            Log.Information(
                "Codigo de verificacion generado para cambio de email para el usuario ID: {UserId}.",
                user.Id
            );
            var newEmailVerificationCode = await _verificationCodeRepository.CreateAsync(code);
            await _emailService.SendChangeEmailVerificationCodeAsync(
                updateProfileDTO.Email,
                newEmailVerificationCode.Code
            );
            Log.Information(
                "Correo de verificacion enviado para cambio de email para el usuario ID: {UserId}.",
                user.Id
            );
        }
        if (updateProfileDTO.Rut != null)
        {
            if (updateProfileDTO.Rut == user.Rut)
            {
                throw new InvalidOperationException("El nuevo RUT no puede ser igual al actual.");
            }
            bool rutExists = await _userRepository.ExistsByRutAsync(updateProfileDTO.Rut);
            if (rutExists)
            {
                throw new InvalidOperationException("El RUT ya está en uso.");
            }
        }
        updateProfileDTO.Adapt(user);

        bool updateResult = await _userRepository.UpdateAsync(user);
        if (!updateResult)
        {
            Log.Error(
                "Error al actualizar el perfil de usuario para el usuario ID: {UserId}",
                userId
            );
            throw new Exception("Error al actualizar el perfil de usuario.");
        }
        return "Perfil de usuario actualizado exitosamente.";
    }

    /// <summary>
    /// Obtiene una lista paginada y filtrada de usuarios para el administrador.
    /// </summary>
    /// <param name="searchParams">Parámetros de búsqueda para filtrar usuarios.</param>
    /// <returns>Una lista paginada y filtrada de usuarios para el administrador.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task<ListedUsersForAdminDTO> GetFilteredForAdminAsync(UserSearchParamsDTO searchParams)
    {
        if (searchParams.PageNumber < 1)
        {
            throw new ArgumentOutOfRangeException("El número de página debe ser mayor o igual a 1.");
        }
        if (searchParams.PageSize < 1)
        {
            throw new ArgumentOutOfRangeException("El tamaño de página debe ser mayor o igual a 1.");
        }
        Log.Information("Obteniendo usuarios filtrados para admin.");
        var (users, totalCount) = await _userRepository.GetFilteredForAdminAsync(searchParams);

        var totalPages = (int)
            Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
        int currentPage = searchParams.PageNumber;
        int pageSize = searchParams.PageSize ?? _defaultPageSize;
        if (currentPage < 1 || currentPage > totalPages)
        {
            throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");
        }
        Log.Information("Total de usuarios encontrados: {TotalCount}", totalCount);

        return new ListedUsersForAdminDTO
        {
            Users = users.Adapt<List<UserForAdminDTO>>(),
            TotalCount = totalCount,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    /// <summary>
    /// Obtiene los detalles de un usuario por su ID para el administrador.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <returns>Detalles del usuario.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<UserDetailsForAdminDTO> GetByIdAsync(int userId)
    {
        User? user =
            await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");
        var userDetails = user.Adapt<UserDetailsForAdminDTO>();
        userDetails.RoleName = await _userRepository.GetUserRoleAsync(user);
        return userDetails;
    }

    /// <summary>
    /// Actualiza el estado de un usuario.
    /// </summary>
    /// <param name="adminId">ID del administrador que realiza el cambio.</param>
    /// <param name="userId">ID del usuario cuyo estado se va a actualizar.</param>
    /// <param name="updateUserStatusDTO">Objeto que contiene la información del nuevo estado.</param>
    /// <returns>Un mensaje indicando el resultado de la operación.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<string> UpdateUserStatusAsync(int adminId, int userId, UpdateUserStatusDTO updateUserStatusDTO)
    {
        if (adminId == userId)
        {
            throw new InvalidOperationException("Un administrador no puede cambiar su propio estado.");
        }
        Log.Information("Buscando usuario para actualizar estado, Usuario ID: {UserId}", userId);
        User? user =
            await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");
        if (!Enum.TryParse(updateUserStatusDTO.Status, out UserStatus newStatus))
        {
            throw new ArgumentException("Estado inválido.");
        }

        Log.Information("Estado del usuario actualizado para el usuario ID: {UserId}", userId);

        var auditLog = new AuditLog
        {
            ChangedBy = adminId,
            UserId = userId,
            Reason = updateUserStatusDTO.Reason ?? "No especificado",
            NewStatus = newStatus,
            PreviousStatus = user.Status,
            ChangedAt = DateTime.UtcNow,
            ActionType = ActionType.StatusChange,
            User = user
        };

        var auditLogResult = await _auditLogRepository.CreateAsync(auditLog);
        if (!auditLogResult)
        {
            Log.Error(
                "Error al crear el registro de auditoría para el cambio de estado del usuario ID: {UserId}",
                userId
            );
            throw new Exception("Error al crear el registro de auditoría.");
        }

        Log.Information("Actualizando estado del usuario ID: {UserId} a {NewState}", userId, newStatus);
        user.Status = newStatus;

        bool updateResult = await _userRepository.UpdateAsync(user);
        if (!updateResult)
        {
            Log.Error(
                "Error al actualizar el estado del usuario para el usuario ID: {UserId}",
                userId
            );
            throw new Exception("Error al actualizar el estado del usuario.");
        }
        return "Estado del usuario actualizado exitosamente.";
    }

    public async Task<string> UpdateUserRoleAsync(int adminId, int userId, UpdateUserRoleDTO updateUserRoleDTO)
    {
        var notLastAdmin = await _userRepository.HasAdminsAsync();
        if (adminId == userId && !notLastAdmin)
        {
            throw new InvalidOperationException("Un administrador no puede cambiar su propio rol si es que es el unico administrador.");
        }
        Log.Information("Buscando usuario para actualizar rol, Usuario ID: {UserId}", userId);
        User? user =
            await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var auditLog = new AuditLog
        {
            ChangedBy = adminId,
            UserId = userId,
            NewRole = updateUserRoleDTO.NewRole,
            PreviousRole = await _userRepository.GetUserRoleAsync(user),
            ChangedAt = DateTime.UtcNow,
            ActionType = ActionType.RoleChange,
            User = user
        };

        Log.Information("Guardando datos de auditoria para el usuario ID: {UserId}", userId);


        var auditLogResult = await _auditLogRepository.CreateAsync(auditLog);
        if (!auditLogResult)
        {
            Log.Error(
                "Error al crear el registro de auditoría para el cambio de rol del usuario ID: {UserId}",
                userId
            );
            throw new Exception("Error al crear el registro de auditoría.");
        }

        Log.Information("Actualizando rol del usuario ID: {UserId} a {NewRole}", userId, updateUserRoleDTO.NewRole);
        bool roleUpdateResult = await _userRepository.UpdateUserRoleAsync(user, updateUserRoleDTO.NewRole);
        if (!roleUpdateResult)
        {
            Log.Error(
                "Error al actualizar el rol del usuario para el usuario ID: {UserId}",
                userId
            );
            throw new Exception("Error al actualizar el rol del usuario.");
        }

        return "Rol del usuario actualizado exitosamente.";
    }
}