using System;

namespace CondoAdmin.Application.DTO.MaintenanceRequest.ListMaintenance;

public class ListMaintenanceOutput
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? UnitNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool IsResolved => ResolvedAt != null;
}
