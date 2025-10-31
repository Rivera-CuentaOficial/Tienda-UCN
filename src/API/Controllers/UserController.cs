using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    }
}