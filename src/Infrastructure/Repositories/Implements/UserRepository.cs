using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiendaUCN.Application.Infrastructure.Repositories.Interfaces;
using TiendaUCN.Domain.Models;
using TiendaUCN.Infrastructure.Data;

namespace TiendaUCN.Application.Infrastructure.Repositories.Implements;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;

    public UserRepository(DataContext dataContext, UserManager<User> userManager)
    {
        _context = dataContext;
        _userManager = userManager;
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
}