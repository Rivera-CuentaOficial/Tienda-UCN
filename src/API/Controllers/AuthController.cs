using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.API.Controllers;
using TiendaUCN.src.Application.DTOs.AuthResponse;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.Services.Interfaces;

public class AuthController(IUserService userService) : BaseController
{
    private readonly IUserService _userService = userService;

    /// <summary>
    /// Inicia sesión en la aplicación y devuelve un token JWT.
    /// </summary>
    /// <param name="loginDTO">DTO con las credenciales de inicio de sesión.</param>
    /// <returns>Un token JWT si el inicio de sesión es exitoso.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
    {
        var token = await _userService.LoginAsync(loginDTO, HttpContext);
        return Ok(new GenericResponse<string>("Login exitoso", token));
    }

    /// <summary>
    /// Registra un nuevo usuario en la aplicación.
    /// </summary>
    /// <param name="registerDTO">DTO con los datos de registro del nuevo usuario.</param>
    /// <returns>Un mensaje de éxito si el registro es exitoso.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
    {
        var message = await _userService.RegisterAsync(registerDTO, HttpContext);
        return Ok(new GenericResponse<string>("Registro exitoso", message));
    }

    /// <summary>
    /// Verifica el correo electrónico del usuario.
    /// </summary>
    /// <param name="verifyEmailDTO">DTO con los datos de verificación del correo electrónico.</param>
    /// <returns>Un mensaje de éxito si la verificación es exitosa.</returns>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyDTO verifyEmailDTO)
    {
        var message = await _userService.VerifyEmailAsync(verifyEmailDTO, HttpContext);
        return Ok(new GenericResponse<string>("Verificación exitosa", message));
    }

    /// <summary>
    /// Reenvía el código de verificación al correo electrónico del usuario.
    /// </summary>
    /// <param name="resendVerifyDTO">DTO con los datos para reenviar la verificación del correo electrónico.</param>
    /// <returns>Un mensaje de éxito si el reenvío es exitoso.</returns>
    [HttpPost("resend-verify-email")]
    public async Task<IActionResult> ResendVerifyEmail([FromBody] ResendVerifyDTO resendVerifyDTO)
    {
        var message = await _userService.ResendVerifyEmail(resendVerifyDTO);
        return Ok(new GenericResponse<string>("Código reenviado exitosamente", message));
    }

    /// <summary>
    /// Envía un correo electrónico para recuperar la contraseña.
    /// </summary>
    /// <param name="recoverPasswordDTO">DTO con los datos para recuperar la contraseña.</param>
    /// <returns>Un mensaje de éxito si el envío es exitoso.</returns>
    [HttpPost("recover-password")]
    public async Task<IActionResult> SendPasswordRecoveryEmail(
        [FromBody] RecoverPasswordDTO recoverPasswordDTO
    )
    {
        var message = await _userService.SendPasswordRecoveryEmail(recoverPasswordDTO, HttpContext);
        return Ok(new GenericResponse<string>("Email enviado exitosamente", message));
    }

    /// <summary>
    /// Restablece la contraseña del usuario.
    /// </summary>
    /// <param name="resetPasswordDTO">DTO con los datos para restablecer la contraseña.</param>
    /// <returns>Un mensaje de éxito si el restablecimiento es exitoso.</returns>
    [HttpPatch("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
    {
        var message = await _userService.ChangeUserPasswordByEmailAsync(resetPasswordDTO);
        return Ok(new GenericResponse<string>("Contraseña cambiada exitosamente", message));
    }
}