namespace CondoAdmin.Domain.Entities;

public class Building
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public int TotalUnits { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navegación
    public ICollection<Unit> Units { get; set; } = [];
}