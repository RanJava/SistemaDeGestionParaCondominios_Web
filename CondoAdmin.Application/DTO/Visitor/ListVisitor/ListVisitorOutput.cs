using System;

namespace CondoAdmin.Application.DTO.Visitors.ListVisitor;

public class ListVisitorOutput
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? DNI { get; set; }
    public string? LicensePlate { get; set; }
    public string? UnitNumber { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public bool IsInside => ExitTime == null;
}
