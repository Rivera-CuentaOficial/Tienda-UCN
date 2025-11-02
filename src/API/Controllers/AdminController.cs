using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.DTOs.UserResponse;
using TiendaUCN.src.Application.DTOs.UserResponse.AdminDTO;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.API.Controllers
{
    public class AdminController : BaseController
    {
        private readonly IUserService _userService;
        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Obtener todos los usuarios con filtros para el administrador.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar usuarios.</param>
        /// <returns>Lista de usuarios filtrados.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUserAsync([FromQuery] UserSearchParamsDTO searchParams)
        {
            var result = await _userService.GetFilteredForAdminAsync(searchParams);
            if (result == null || result.Users.Count == 0)
            {
                throw new KeyNotFoundException("No se encontraron usuarios.");
            }
            return Ok(
                new GenericResponse<ListedUsersForAdminDTO>(
                    "Usuarios obtenidos exitosamente",
                    result
                )
            );
        }

        /// <summary>
        /// Obtener un usuario por su ID.
        /// </summary>
        /// <param name="id">ID del usuario.</param>
        /// <returns>Detalles del usuario.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserByIdAsync(int id)
        {
            var result = await _userService.GetByIdAsync(id);
            if (result == null)
            {
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            return Ok(
                new GenericResponse<UserDetailsForAdminDTO>(
                    "Usuario obtenido exitosamente",
                    result
                )
            );
        }

        [HttpPatch("users/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserStatusAsync(int id, [FromBody] UpdateUserStatusDTO updateUserStatusDTO)
        {
            var adminId =
                (
                    User.Identity?.IsAuthenticated == true
                        ? User
                            .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                            ?.Value
                        : null
                ) ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
            int.TryParse(adminId, out int parsedAdminId);
            var result = await _userService.UpdateUserStatusAsync(parsedAdminId, id, updateUserStatusDTO);
            return Ok(
                new GenericResponse<string>("Estado del usuario actualizado exitosamente", result)
            );
        }

        [HttpPatch("users/{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRoleAsync(int id, [FromBody] UpdateUserRoleDTO updateUserRoleDTO)
        {
            var adminId =
                (
                    User.Identity?.IsAuthenticated == true
                        ? User
                            .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                            ?.Value
                        : null
                ) ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
            int.TryParse(adminId, out int parsedAdminId);
            var result = await _userService.UpdateUserRoleAsync(parsedAdminId, id, updateUserRoleDTO);
            return Ok(
                new GenericResponse<string>("Rol del usuario actualizado exitosamente", result)
            );
        }
    }
}