using System;

namespace CondoAdmin.Application.DTO.Building.ListBuilding;

public class ListBuildingOutput
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public int TotalUnits { get; set; }
    public bool IsActive { get; set; }
}
