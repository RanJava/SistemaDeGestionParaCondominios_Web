using System;

namespace CondoAdmin.Application.DTO.Unit.CreateUnit;

public class CreateUnitOutput
{
    public int Id { get; set; }
    public string? UnitNumber { get; set; }
    public int Floor { get; set; }
    public decimal MonthlyFee { get; set; }
    public string? BuildingName { get; set; }
}
