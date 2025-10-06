using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.API.Controllers;
using TiendaUCN.src.Application.DTOs.AuthResponse;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.Services.Interfaces;

public class AuthController(IUserService userService) : BaseController
{
    private readonly IUserService _userService = userService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
    {
        var token = await _userService.LoginAsync(loginDTO, HttpContext);
        return Ok(new GenericResponse<string>("Login exitoso", token));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
    {
        var message = await _userService.RegisterAsync(registerDTO, HttpContext);
        return Ok(new GenericResponse<string>("Registro exitoso", message));
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyDTO verifyEmailDTO)
    {
        var message = await _userService.VerifyEmailAsync(verifyEmailDTO, HttpContext);
        return Ok(new GenericResponse<string>("Verificación exitosa", message));
    }

    [HttpPost("resend-verify-email")]
    public async Task<IActionResult> ResendVerifyEmail([FromBody] ResendVerifyDTO resendVerifyDTO)
    {
        var message = await _userService.ResendVerifyEmail(resendVerifyDTO);
        return Ok(new GenericResponse<string>("Código reenviado exitosamente", message));
    }

    [HttpPost("recover-password")]
    public async Task<IActionResult> SendPasswordRecoveryEmail(
        [FromBody] RecoverPasswordDTO recoverPasswordDTO
    )
    {
        var message = await _userService.SendPasswordRecoveryEmail(recoverPasswordDTO, HttpContext);
        return Ok(new GenericResponse<string>("Email enviado exitosamente", message));
    }

    [HttpPatch("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
    {
        var message = await _userService.ChangeUserPasswordByEmailAsync(resetPasswordDTO);
        return Ok(new GenericResponse<string>("Contraseña cambiada exitosamente", message));
    }
}