using System;

namespace CondoAdmin.Application.DTO.Visitors.UpdateVisitor;

public class UpdateVisitorInput
{
    public required string FullName { get; set; }
    public required string DNI { get; set; }
    public string? LicensePlate { get; set; }
    public int UnitId { get; set; }
}
