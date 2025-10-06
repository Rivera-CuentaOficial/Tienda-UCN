namespace TiendaUCN.src.Application.Services.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeEmailAsync(string email, string code);
    Task SendWelcomeEmailAsync(string email);
    Task SendPasswordRecoveryEmail(string email, string code);
    Task<string> LoadTemplate(string templateName, string? code);
}