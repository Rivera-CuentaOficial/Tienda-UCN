namespace TiendaUCN.src.Domain.Models;

public enum CodeType
{
    EmailVerification,
    PasswordReset,
    PasswordChange,
}

public class VerificationCode
{
    public int Id { get; set; }
    public required CodeType Type { get; set; }
    public required string Code { get; set; }
    public int AttemptCount { get; set; } = 0;
    public required DateTime Expiration { get; set; }
    public required int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}