using Microsoft.AspNetCore.Identity;

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
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}