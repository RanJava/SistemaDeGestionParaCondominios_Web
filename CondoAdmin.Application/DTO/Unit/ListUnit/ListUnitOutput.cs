using System;

namespace CondoAdmin.Application.DTO.Unit.ListUnit;

public class ListUnitOutput
{
    public int Id { get; set; }
    public string? UnitNumber { get; set; }
    public int Floor { get; set; }
    public decimal AreaM2 { get; set; }
    public decimal MonthlyFee { get; set; }
    public string? Status { get; set; }
    public string? BuildingName { get; set; }
}
