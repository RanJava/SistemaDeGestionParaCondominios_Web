using System;

namespace CondoAdmin.Application.DTO.Building.AddBuilding;

public class AddBuildingInput
{
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public required int TotalUnits { get; set; }
}
