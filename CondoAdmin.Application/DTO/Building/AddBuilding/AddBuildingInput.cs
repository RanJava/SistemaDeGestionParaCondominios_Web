using System;

namespace CondoAdmin.Application.DTO.Building.AddBuilding;

public class AddBuildingInput
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
}
