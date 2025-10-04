using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Resend;
using Serilog;
using TiendaUCN.Application.Services.Interfaces;

namespace TiendaUCN.Application.Services.Implements;

public class EmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public EmailService(
        IResend resend,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment
    )
    {
        _resend = resend;
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task SendVerificationCodeEmailAsync(string email, string code)
    {
        var htmlBody = await LoadTemplate("VerificationCode", code);
        var message = new EmailMessage
        {
            From =
                _configuration.GetValue<string>("EmailConfiguration:From")
                ?? throw new InvalidOperationException("Email 'From' no esta configurado."),
            To = email,
            Subject =
                _configuration.GetValue<string>("EmailConfiguration:VerificationSubject")
                ?? throw new InvalidOperationException(
                    "Email 'VerificationSubject' no esta configurado."
                ),
            HtmlBody = htmlBody,
        };
        await _resend.EmailSendAsync(message);
    }

    public async Task SendWelcomeEmailAsync(string email)
    {
        var htmlBody = await LoadTemplate("Welcome", null);
        var message = new EmailMessage
        {
            From =
                _configuration.GetValue<string>("EmailConfiguration:From")
                ?? throw new InvalidOperationException("Email 'From' no esta configurado."),
            To = email,
            Subject =
                _configuration.GetValue<string>("EmailConfiguration:WelcomeSubject")
                ?? throw new InvalidOperationException(
                    "Email 'WelcomeSubject' no esta configurado."
                ),
            HtmlBody = htmlBody,
        };
        await _resend.EmailSendAsync(message);
    }

    public async Task SendPasswordRecoveryEmail(string email, string code)
    {
        var htmlBody = await LoadTemplate("PasswordRecovery", code);
        var message = new EmailMessage
        {
            From =
                _configuration.GetValue<string>("EmailConfiguration:From")
                ?? throw new InvalidOperationException("Email 'From' no esta configurado."),
            To = email,
            Subject =
                _configuration.GetValue<string>("EmailConfiguration:PasswordRecoverySubject")
                ?? throw new InvalidOperationException(
                    "Email 'PasswordRecoverySubject' no esta configurado."
                ),
            HtmlBody = htmlBody,
        };
        await _resend.EmailSendAsync(message);
    }

    public async Task<string> LoadTemplate(string templateName, string? code)
    {
        var template = Path.Combine(
            _webHostEnvironment.ContentRootPath,
            "src",
            "Application",
            "Templates",
            "Emails",
            $"{templateName}.html"
        );
        var htmlContent = await File.ReadAllTextAsync(template);
        return htmlContent.Replace("{{CODE}}", code);
    }
}