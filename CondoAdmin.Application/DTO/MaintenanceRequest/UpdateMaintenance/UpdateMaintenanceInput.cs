using System;

namespace CondoAdmin.Application.DTO.MaintenanceRequest.UpdateMaintenance;

public class UpdateMaintenanceInput
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int UnitId { get; set; }
}
