using TiendaUCN.Application.DTOs.AuthResponse;

namespace TiendaUCN.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<string> LoginAsync(LoginDTO loginDTO, HttpContext httpContext);
        Task<string> RegisterAsync(RegisterDTO registerDTO, HttpContext httpContext);
        Task<string> VerifyEmailAsync(VerifyDTO verifyEmailDTO, HttpContext httpContext);
        Task<string> ResendVerifyEmail(ResendVerifyDTO resendVerifyDTO);
    }
}