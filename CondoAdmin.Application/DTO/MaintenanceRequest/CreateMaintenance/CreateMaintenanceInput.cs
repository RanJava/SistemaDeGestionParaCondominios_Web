using System;

namespace CondoAdmin.Application.DTO.MaintenanceRequest.CreateMaintenance;

public class CreateMaintenanceInput
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int UnitId { get; set; }
}
