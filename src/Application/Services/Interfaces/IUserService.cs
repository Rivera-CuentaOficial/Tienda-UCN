using TiendaUCN.src.Application.DTOs.AuthResponse;


namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<string> LoginAsync(LoginDTO loginDTO, HttpContext httpContext);
        Task<string> RegisterAsync(RegisterDTO registerDTO, HttpContext httpContext);
        Task<string> VerifyEmailAsync(VerifyDTO verifyEmailDTO, HttpContext httpContext);
        Task<string> ResendVerifyEmail(ResendVerifyDTO resendVerifyDTO);
        Task<string> SendPasswordRecoveryEmail(
            RecoverPasswordDTO recoverPasswordDTO,
            HttpContext httpContext
        );
        Task<string> ChangeUserPasswordByEmailAsync(ResetPasswordDTO resetPasswordDTO);
        string NormalizePhoneNumber(string PhoneNumber);
        Task<int> DeleteUnconfirmedAsync();

    }
}