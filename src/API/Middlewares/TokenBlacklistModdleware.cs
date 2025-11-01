using Serilog;
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
            Log.Information($"Verificando estado del token {authHeader}.", authHeader);
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Replace("Bearer ", "");
                var isBlacklisted = await tokenService.IsTokenBlacklisted(token);
                if (isBlacklisted)
                {
                    Log.Warning("Intento de acceso con token bloqueado.");
                    var statusCode = StatusCodes.Status401Unauthorized;
                    string result = $"Codigo de estado = {statusCode}";
                    context.Response.StatusCode = statusCode;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new GenericResponse<object>(
                        "Acceso no autorizado, token de acceso revocado",
                        result
                    ));
                    return;
                }
            }
            await _next(context);
        }
    }
}