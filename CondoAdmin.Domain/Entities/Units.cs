namespace CondoAdmin.Domain.Entities;

public class Unit
{
    public int Id { get; set; }
    public required string UnitNumber { get; set; }   
    public int Floor { get; set; }
    public decimal AreaM2 { get; set; }
    public decimal MonthlyFee { get; set; }
    public bool Status { get; set; } 

    // FK
    public int BuildingId { get; set; }
    public Building Building { get; set; } = null!;

    // Navegación
    public ICollection<Resident> Residents { get; set; } = [];
    public ICollection<Visitor> Visitors { get; set; } = [];
    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = [];
}