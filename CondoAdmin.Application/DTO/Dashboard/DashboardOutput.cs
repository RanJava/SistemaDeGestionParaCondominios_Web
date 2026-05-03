using System;

namespace CondoAdmin.Application.DTO.Dashboard;

public class DashboardOutput
{
    public BuildingSummary Buildings { get; set; } = new();
    public UnitSummary Units { get; set; } = new();
    public ResidentSummary Residents { get; set; } = new();
    public PaymentSummary Payments { get; set; } = new();
    public MaintenanceSummary Maintenance { get; set; } = new();
}
public class BuildingSummary
{
    public int Total { get; set; }
    public int Active { get; set; }
}

public class UnitSummary
{
    public int Total { get; set; }
    public int Available { get; set; }
    public int Sold { get; set; }
    public int Rented { get; set; }
}

public class ResidentSummary
{
    public int Total { get; set; }
    public int Active { get; set; }
}

public class PaymentSummary
{
    public int PendingCount { get; set; }
    public decimal TotalDebt { get; set; }
}

public class MaintenanceSummary
{
    public int PendingCount { get; set; }
    public int ResolvedCount { get; set; }
}
