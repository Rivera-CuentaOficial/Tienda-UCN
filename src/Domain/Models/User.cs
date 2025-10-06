using Bogus.DataSets;
using Microsoft.AspNetCore.Identity;

namespace TiendaUCN.src.Domain.Models;

public enum Gender
{
    Masculino,
    Femenino,
    Otro,
}

public class User : IdentityUser<int>
{
    public required string Rut { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required Gender Gender { get; set; }
    public required DateTime BirthDate { get; set; }
    public ICollection<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}