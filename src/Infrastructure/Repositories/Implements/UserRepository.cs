using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;


namespace TiendaUCN.src.Infrastructure.Repositories.Implements;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IVerificationCodeRepository _verificationCodeRepository;
    private readonly int _daysOfDeleteUnconfirmedUsers;

    public UserRepository(DataContext dataContext, UserManager<User> userManager, IVerificationCodeRepository verificationCodeRepository, IConfiguration configuration)
    {


        _context = dataContext;
        _userManager = userManager;
        _verificationCodeRepository = verificationCodeRepository;
        _daysOfDeleteUnconfirmedUsers = configuration.GetValue<int?>("Jobs:DaysOfDeleteUnconfirmedUsers") ?? throw new InvalidOperationException("La configuración 'Jobs:DaysOfDeleteUnconfirmedUsers' no está definida.");

    }

    public async Task<User> GetByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<string> GetUserRoleAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault()!;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByRutAsync(string rut)
    {
        return await _context.Users.AnyAsync(u => u.Rut == rut);
    }

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

    public async Task<bool> ConfirmEmailAsync(string email)
    {
        var result = await _context
            .Users.Where(u => u.Email == email)
            .ExecuteUpdateAsync(u => u.SetProperty(user => user.EmailConfirmed, true));
        return result > 0;
    }

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

    public async Task<bool> DeleteAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var result = await _userManager.DeleteAsync(user!);
        return result.Succeeded;
    }
    public async Task<int> DeleteUnconfirmedAsync()
    {
        Log.Information("Iniciando eliminación de usuarios no confirmados");

        var cutoffDate = DateTime.UtcNow.AddDays(_daysOfDeleteUnconfirmedUsers);

        var unconfirmedUsers = await _context.Users
            .Where(u => !u.EmailConfirmed && u.RegisteredAt < cutoffDate)
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
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }
}