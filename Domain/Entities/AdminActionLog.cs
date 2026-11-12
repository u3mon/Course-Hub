namespace Domain.Entities;

public class AdminActionLog
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public string AdminEmail { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string EntityName { get; set; } = null!;
    public int? EntityId { get; set; }
    public string Details { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
