using System;

namespace CondoAdmin.Application.DTO.Report.DebtorReport;

public class DebtorReportOutput
{
    public string? BuildingName { get; set; }
    public int TotalDebtors { get; set; }
    public decimal TotalDebt { get; set; }
    public List<DebtorDetail> Debtors { get; set; } = [];
}

public class DebtorDetail
{
    public string? ResidentName { get; set; }
    public string? DNI { get; set; }
    public string? UnitNumber { get; set; }
    public int PendingPayments { get; set; }
    public decimal TotalDebt { get; set; }
}
