using TiendaUCN.src.Application.DTOs.AuthResponse;
using TiendaUCN.src.Application.DTOs.UserResponse;
using TiendaUCN.src.Application.DTOs.UserResponse.AdminDTO;
using TiendaUCN.src.Domain.Models;

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
        Task<string> ChangeUserPasswordAsync(string token, int userId, DateTime tokenExpiration, ChangePasswordDTO changePasswordDTO);
        string NormalizePhoneNumber(string PhoneNumber);
        Task<int> DeleteUnconfirmedAsync();
        Task<ViewUserProfileDTO> GetUserProfileAsync(int userId);
        Task<string> UpdateUserProfileAsync(int userId, UpdateProfileDTO updateProfileDTO);
        Task<ListedUsersForAdminDTO> GetFilteredForAdminAsync(UserSearchParamsDTO searchParams);
        Task<UserDetailsForAdminDTO> GetByIdAsync(int id);
        Task<string> UpdateUserStatusAsync(int adminId, int userId, UpdateUserStatusDTO updateUserStatusDTO);
        Task<string> UpdateUserRoleAsync(int adminId, int userId, UpdateUserRoleDTO updateUserRoleDTO);
    }
}