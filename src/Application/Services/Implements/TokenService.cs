using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Services.Implements;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    private readonly string _jwtSecret;

    public TokenService(IConfiguration configuration)
    {
        _configuration =
            configuration
            ?? throw new ArgumentNullException(
                nameof(configuration),
                "La configuración no puede ser nula."
            );
        _jwtSecret =
            _configuration["JWTSecret"]
            ?? throw new InvalidOperationException("La clave JWTSecret no está configurada.");
    }

    public string CreateToken(User user, string rolename, bool rememberMe)
    {
        try
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, rolename),
                new Claim(ClaimTypes.Email, user.Email!),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSecret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(rememberMe ? 24 : 1),
                signingCredentials: creds
            );

            Log.Information("Token JWT creado para el usuario {UserId}", user.Id);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creando el token JWT para el usuario {UserId}", user.Id);
            throw new InvalidOperationException("Error creando el token JWT.", ex);
        }
    }
}