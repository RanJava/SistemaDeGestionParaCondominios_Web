using System;

namespace CondoAdmin.Application.DTO.Building.AddBuilding;

public class AddBuildingOutput
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
     public int TotalUnits { get; set; }

}
