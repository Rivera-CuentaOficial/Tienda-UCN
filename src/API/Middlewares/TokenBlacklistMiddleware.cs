using Serilog;
using System.Text.Json;
using TiendaUCN.src.Application.DTOs.BaseResponse;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.API.Middlewares
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenBlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Replace("Bearer ", "");
                /*var isBlacklisted = await tokenService.IsTokenBlacklisted(token);
                if (isBlacklisted)
                {
                    Log.Warning("Intento de acceso con token bloqueado.");

                    throw new UnauthorizedAccessException("El token ha sido bloqueado.");

                    /*ErrorDetail error = new ErrorDetail("No autorizado", "El token ha sido bloqueado.");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var json = JsonSerializer.Serialize(
                            error,
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    
                    await context.Response.WriteAsync(json);

                    return;
                }*/
            }
            await _next(context);
        }
    }
}