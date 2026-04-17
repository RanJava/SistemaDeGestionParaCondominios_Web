using System.Text.Json.Serialization;

namespace CondoAdmin.Domain.Entities;

public class Building
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public int TotalUnits { get; set; }
    [JsonIgnore]
    public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navegación
    [JsonIgnore]
    public ICollection<Unit> Units { get; set; } = [];
}