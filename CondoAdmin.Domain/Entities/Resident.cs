namespace CondoAdmin.Domain.Entities;

public class Resident
{
    public int Id { get; set; }
    public required string FirstName { get; set; } 
    public required string LastName { get; set; } 
    public required string Email { get; set; } 
    public required string Phone { get; set; } 
    public required string DNI { get; set; } 
    public DateTime MoveInDate { get; set; }
    public DateTime? MoveOutDate { get; set; }
    public bool IsActive { get; set; } = true;

    // FK
    public int? UnitId { get; set; }
    public Unit? Unit { get; set; } = null!;

    // Navegación
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<Sale> Sales { get; set; } = [];
}