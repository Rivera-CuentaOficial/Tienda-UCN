using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.DTOs.UserResponse;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.API.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Ver el perfil de un usuario.
        /// </summary>
        /// <returns>Datos del perfil del usuario</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> ViewUserProfileAsync()
        {
            var userId =
                (
                    User.Identity?.IsAuthenticated == true
                        ? User
                            .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                            ?.Value
                        : null
                ) ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
            int.TryParse(userId, out int parsedUserId);
            var result = await _userService.GetUserProfileAsync(parsedUserId);
            return Ok(
                new GenericResponse<ViewUserProfileDTO>(
                    "Perfil de usuario obtenido exitosamente",
                    result
                )
            );
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfileAsync(
            [FromBody] UpdateProfileDTO updateProfileDTO
        )
        {
            var userId =
                (
                    User.Identity?.IsAuthenticated == true
                        ? User
                            .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                            ?.Value
                        : null
                ) ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
            int.TryParse(userId, out int parsedUserId);
            var result = await _userService.UpdateUserProfileAsync(parsedUserId, updateProfileDTO);
            return Ok(
                new GenericResponse<string>("Perfil de usuario actualizado exitosamente", result)
            );
        }

        [HttpPatch("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangeUserPasswordAsync(
            [FromBody] ChangePasswordDTO changePasswordDTO
        )
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userId =
                (
                    User.Identity?.IsAuthenticated == true
                        ? User
                            .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                            ?.Value
                        : null
                ) ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
            int.TryParse(userId, out int parsedUserId);
            var expiration = User.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            Log.Information($"La fecha de expiracion es: {expiration}", expiration);
            if (!long.TryParse(expiration, out long parsedExpiration))
            {
                throw new ArgumentOutOfRangeException("El token no tiene una fecha de expiración válida.");
            }
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(parsedExpiration).UtcDateTime;
            Log.Information($"La fecha de expiracion parseada es: {expiresAt}", expiresAt);
            var result = await _userService.ChangeUserPasswordAsync(
                token,
                parsedUserId,
                expiresAt,
                changePasswordDTO
            );
            return Ok(new GenericResponse<string>("Contraseña cambiada exitosamente", result));
        }
    }
}