using System;

namespace CondoAdmin.Application.DTO.Visitors.CreateVisitor;

public class CreateVisitorOutput
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? DNI { get; set; }
    public string? UnitNumber { get; set; }
    public DateTime EntryTime { get; set; }
}
