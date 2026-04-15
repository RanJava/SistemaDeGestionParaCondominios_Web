namespace CondoAdmin.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
    public required string Month { get; set; } 

    // FK
    public int ResidentId { get; set; }
    public Resident Resident { get; set; } = null!;
}