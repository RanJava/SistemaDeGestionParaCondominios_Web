using System;

namespace CondoAdmin.Application.DTO.Visitors.CreateVisitor;

public class CreateVisitorInput
{
    public required string FullName { get; set; }
    public required string DNI { get; set; }
    public string? LicensePlate { get; set; }
    public int UnitId { get; set; }
}
