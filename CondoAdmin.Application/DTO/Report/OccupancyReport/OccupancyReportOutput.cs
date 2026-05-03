using System;

namespace CondoAdmin.Application.DTO.Report.OccupancyReport;

public class OccupancyReportOutput
{
    public string? BuildingName { get; set; }
    public int TotalUnits { get; set; }
    public int Available { get; set; }
    public int Sold { get; set; }
    public int Rented { get; set; }
    public decimal OccupancyRate { get; set; }
}