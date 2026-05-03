using System;

namespace CondoAdmin.Application.DTO.Unit.UpdateUnit;

public class UpdateUnitInput
{
    public required string UnitNumber { get; set; }
    public int Floor { get; set; }
    public decimal AreaM2 { get; set; }
    public decimal MonthlyFee { get; set; }
    public int BuildingId { get; set; }
}
