namespace CondoAdmin.Domain.Entities;

public class Visitor
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string DNI { get; set; } 
    public string? LicensePlate { get; set; }
    public DateTime EntryTime { get; set; } = DateTime.UtcNow;
    public DateTime? ExitTime { get; set; }

    // FK
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;
}