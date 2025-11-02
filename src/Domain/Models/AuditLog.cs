namespace TiendaUCN.src.Domain.Models
{
    public enum UserStatus
    {
        Active,
        Blocked,
    }
    public enum ActionType
    {
        StatusChange,
        RoleChange
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public required int UserId { get; set; }
        public required int ChangedBy { get; set; }
        public UserStatus? NewStatus { get; set; }
        public UserStatus? PreviousStatus { get; set; }
        public string? NewRole { get; set; }
        public string? PreviousRole { get; set; }
        public required DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? Reason { get; set; }
        public required ActionType ActionType { get; set; }
        public required User User { get; set; }
    }
}