using System;

namespace CondoAdmin.Application.DTO.Residents.CreateResident;

public class CreateResidentOutput
{
    public int Id {get; set;}
    public string? FullName {get; set;}
    public string? DNI {get; set;}
    public string? UnitNumber {get; set;}
    public DateTime MoveInDate {get; set;}
}
