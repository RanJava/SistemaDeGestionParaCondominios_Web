using System;

namespace CondoAdmin.Application.DTO.Report.IncomeReport;

public class IncomeReportOutput
{
    public string? BuildingName { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalPending { get; set; }
    public List<IncomeDetail> Details { get; set; } = [];
}

public class IncomeDetail
{
    public string? Month { get; set; }
    public decimal Collected { get; set; }
    public decimal Pending { get; set; }
}
