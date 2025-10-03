namespace TiendaUCN.Application.Services.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeEmailAsync(string email, string code);
    Task SendWelcomeEmailAsync(string email);
    Task<string> LoadTemplate(string templateName, string? code);
}