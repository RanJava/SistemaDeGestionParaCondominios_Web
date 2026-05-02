using System;

namespace CondoAdmin.Application.DTO.Building.UpdateBuilding;

public class UpdateBuildingInput
{
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public int TotalUnits { get; set; }
    public bool IsActive { get; set; }
}
