using System;

namespace CondoAdmin.Application.DTO.MaintenanceRequest.CreateMaintenance;

public class CreateMaintenanceOutput
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? UnitNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}
