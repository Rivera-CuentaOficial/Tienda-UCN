using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TiendaUCN.src.Application.DTOs.UserResponse;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly IVerificationCodeRepository _verificationCodeRepository;
    private readonly int _daysOfDeleteUnconfirmedUsers;
    private readonly int _defaultPageSize;

    public UserRepository(
        DataContext dataContext,
        UserManager<User> userManager,
        IVerificationCodeRepository verificationCodeRepository,
        IConfiguration configuration
    )
    {
        _context = dataContext;
        _configuration = configuration;
        _defaultPageSize = _configuration.GetValue<int>("Users:DefaultPageSize");
        _userManager = userManager;
        _verificationCodeRepository = verificationCodeRepository;
        _daysOfDeleteUnconfirmedUsers =
            configuration.GetValue<int?>("Jobs:DaysOfDeleteUnconfirmedUsers")
            ?? throw new InvalidOperationException(
                "La configuración 'Jobs:DaysOfDeleteUnconfirmedUsers' no está definida."
            );
    }

    /// <summary>
    /// Obtiene un usuario por su correo electrónico.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <returns>Usuario correspondiente al correo electrónico.</returns>
    public async Task<User> GetByEmailAsync(string email)
    {
        User? user = await _userManager.FindByEmailAsync(email);
        return user!;
    }

    /// <summary>
    /// Verifica si la contraseña proporcionada coincide con la del usuario.
    /// </summary>
    /// <param name="user">Usuario a verificar.</param>
    /// <param name="password">Contraseña a verificar.</param>
    /// <returns>True si la contraseña es correcta, de lo contrario false.</returns>
    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    /// <summary>
    /// Obtiene el rol del usuario.
    /// </summary>
    /// <param name="user">Usuario del cual obtener el rol.</param>
    /// <returns>Rol del usuario.</returns>
    public async Task<string> GetUserRoleAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault()!;
    }

    /// <summary>
    /// Verifica si un usuario existe por su correo electrónico.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <returns>True si el usuario existe, de lo contrario false.</returns>
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    /// <summary>
    /// Verifica si un usuario existe por su RUT.
    /// </summary>
    /// <param name="rut">RUT del usuario.</param>
    /// <returns>True si el usuario existe, de lo contrario false.</returns>
    public async Task<bool> ExistsByRutAsync(string rut)
    {
        return await _context.Users.AnyAsync(u => u.Rut == rut);
    }

    /// <summary>
    /// Crea un nuevo usuario con la contraseña especificada.
    /// </summary>
    /// <param name="user">Usuario a crear.</param>
    /// <param name="password">Contraseña del usuario.</param>
    /// <returns>True si el usuario fue creado exitosamente, de lo contrario false.</returns>
    public async Task<bool> CreateAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            var userRole = await _userManager.AddToRoleAsync(user, "Customer");
            return userRole.Succeeded;
        }
        return false;
    }

    /// <summary>
    /// Actualiza la información del usuario.
    /// </summary>
    /// <param name="user">Usuario a actualizar.</param>
    /// <returns>True si la actualización fue exitosa, de lo contrario false.</returns>
    public async Task<bool> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            Log.Error(
                "Error al actualizar el usuario: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description))
            );
        }
        return result.Succeeded;
    }

    /// <summary>
    /// Actualiza el rol del usuario.
    /// </summary>
    /// <param name="user">Usuario cuyo rol se va a actualizar.</param>
    /// <param name="newRole">Nuevo rol del usuario.</param>
    /// <returns>True si el rol fue actualizado exitosamente, de lo contrario false.</returns>
    public async Task<bool> UpdateUserRoleAsync(User user, string newRole)
    {
        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            Log.Error(
                "Error al remover roles del usuario: {Errors}",
                string.Join(", ", removeResult.Errors.Select(e => e.Description))
            );
            return false;
        }

        var addResult = await _userManager.AddToRoleAsync(user, newRole);
        if (!addResult.Succeeded)
        {
            Log.Error(
                "Error al agregar nuevo rol al usuario: {Errors}",
                string.Join(", ", addResult.Errors.Select(e => e.Description))
            );
            return false;
        }

        return true;
    }

    /// <summary>
    /// Confirma el correo electrónico del usuario.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <returns>True si el correo electrónico fue confirmado, de lo contrario false.</returns>
    public async Task<bool> ConfirmEmailAsync(string email)
    {
        var result = await _context
            .Users.Where(u => u.Email == email)
            .ExecuteUpdateAsync(u => u.SetProperty(user => user.EmailConfirmed, true));
        return result > 0;
    }

    /// <summary>
    /// Cambia la contraseña del usuario.
    /// </summary>
    /// <param name="user">Usuario cuya contraseña se va a cambiar.</param>
    /// <param name="newPassword">Nueva contraseña del usuario.</param>
    /// <returns>True si la contraseña fue cambiada exitosamente, de lo contrario false.</returns>
    public async Task<bool> ChangeUserPasswordAsync(User user, string newPassword)
    {
        var removePassword = await _userManager.RemovePasswordAsync(user);
        if (removePassword.Succeeded)
        {
            Log.Information("Contraseña antigua removida.");
            var createNewPassword = await _userManager.AddPasswordAsync(user, newPassword);
            if (createNewPassword.Succeeded)
            {
                Log.Information("Nueva contraseña insertada.");
                return createNewPassword.Succeeded;
            }
            ;
        }
        return removePassword.Succeeded;
    }

    /// <summary>
    /// Elimina un usuario por su ID.
    /// </summary>
    /// <param name="userId">ID del usuario a eliminar.</param>
    /// <returns>True si el usuario fue eliminado exitosamente, de lo contrario false.</returns>
    public async Task<bool> DeleteAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var result = await _userManager.DeleteAsync(user!);
        return result.Succeeded;
    }

    /// <summary>
    /// Elimina usuarios no confirmados que exceden el período permitido.
    /// </summary>
    /// <returns>El número de usuarios eliminados.</returns>
    public async Task<int> DeleteUnconfirmedAsync()
    {
        Log.Information("Iniciando eliminación de usuarios no confirmados");

        var cutoffDate = DateTime.UtcNow.AddDays(_daysOfDeleteUnconfirmedUsers);

        var unconfirmedUsers = await _context
            .Users.Where(u => !u.EmailConfirmed && u.RegisteredAt < cutoffDate)
            .Include(u => u.VerificationCodes)
            .ToListAsync();

        if (!unconfirmedUsers.Any())
        {
            Log.Information("No se encontraron usuarios no confirmados para eliminar");
            return 0;
        }

        foreach (var user in unconfirmedUsers)
        {
            if (user.VerificationCodes.Any())
            {
                await _verificationCodeRepository.DeleteByUserIdAsync(user.Id);
            }
        }

        _context.Users.RemoveRange(unconfirmedUsers);
        await _context.SaveChangesAsync();

        Log.Information($"Eliminados {unconfirmedUsers.Count} usuarios no confirmados");
        return unconfirmedUsers.Count;
    }

    /// <summary>
    /// Obtiene un usuario por su ID.
    /// </summary>
    /// <param name="id">ID del usuario a obtener.</param>
    /// <returns>El usuario correspondiente al ID, o null si no se encuentra.</returns>
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    /// <summary>
    /// Obtiene usuarios filtrados para el administrador.
    /// </summary>
    /// <param name="searchParams">Parámetros de búsqueda para filtrar usuarios.</param>
    /// <returns>Una tupla que contiene la lista de usuarios filtrados y el conteo total.</returns>
    public async Task<(IEnumerable<UserWithRoleDTO> users, int totalCount)> GetFilteredForAdminAsync(
        UserSearchParamsDTO searchParams
    )
    {
        var query = _context.Users
            .Join(_context.UserRoles,
                user => user.Id,
                userRole => userRole.UserId,
                (user, userRole) => new { user, userRole })
            .Join(_context.Roles,
                combined => combined.userRole.RoleId,
                role => role.Id,
                (combined, role) => new { combined.user, RoleName = role.Name })
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
        {
            var searchTerm = searchParams.SearchTerm.Trim().ToLower();

            query = query.Where(u =>
                u.user.Email != null && u.user.Email.ToLower().Contains(searchTerm)
            );
        }
        if (!string.IsNullOrWhiteSpace(searchParams.RoleName))
        {
            var roleTerm = searchParams.RoleName.Trim().ToLower();

            query = query.Where(u =>
                u.RoleName != null && u.RoleName.ToLower().Contains(roleTerm)
            );
        }
        if (!string.IsNullOrWhiteSpace(searchParams.State))
        {
            var stateTerm = searchParams.State.Trim().ToLower();

            query = query.Where(u =>
                u.user.Status.ToString().ToLower().Contains(stateTerm)
            );
        }

        var isAscending = searchParams.SortOrder?.ToLower() == "ascending";

        query = (searchParams.SortBy?.ToLower()) switch
        {
            "email" => isAscending
                                ? query.OrderBy(u => u.user.Email!.ToLower())
                                : query.OrderByDescending(u => u.user.Email!.ToLower()),
            "lastloginat" => isAscending
                                ? query.OrderBy(u => u.user.LastLoginAt)
                                : query.OrderByDescending(u => u.user.LastLoginAt),
            _ => isAscending
                                ? query.OrderBy(u => u.user.RegisteredAt)
                                : query.OrderByDescending(u => u.user.RegisteredAt),
        };

        var pageSize = searchParams.PageSize ?? _defaultPageSize;
        var users = await query
            .Skip((searchParams.PageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserWithRoleDTO
            {
                User = u.user,
                RoleName = u.RoleName!
            })
            .ToArrayAsync();
        var totalCount = await query.CountAsync();

        return (users, totalCount);
    }

    /// <summary>
    /// Verifica si existen administradores en el sistema.
    /// </summary>
    /// <returns>True si existen administradores, de lo contrario false.</returns>
    public async Task<bool> HasAdminsAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        return admins.Count > 1;
    }
}