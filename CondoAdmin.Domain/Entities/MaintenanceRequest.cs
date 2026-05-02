namespace CondoAdmin.Domain.Entities;

public class MaintenanceRequest
{
    public int Id { get; set; }
    public required string Title { get; set; } 
    public required string Description { get; set; } 
    public DateTime CreatedAt { get; set; } 
    public DateTime? ResolvedAt { get; set; }

    // FK
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;
}