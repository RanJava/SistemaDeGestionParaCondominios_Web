using System;

namespace CondoAdmin.Application.DTO.Residents.ListResident;

public class ListResidentOutput
{
    public int    Id         { get; set; }
    public string? FullName   { get; set; }
    public string? DNI        { get; set; }
    public string? UnitNumber { get; set; }
}
