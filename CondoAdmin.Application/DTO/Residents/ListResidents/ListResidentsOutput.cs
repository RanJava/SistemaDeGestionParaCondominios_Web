using System;

namespace CondoAdmin.Application.DTO.Residents.ListResidents;

public class ListResidentsOutput
{
    public int Id {get; set;}
    public required string FullName {get; set;}
    public required string Email {get; set;}
    public required string Phone {get; set;}
    public required string DNI {get; set;}
    public DateTime MoveInDate {get; set;}
    public DateTime? MoveOutDate {get; set;}
    public bool IsActive {get; set;} = true;
    public required string UnitNumber {get; set;}
}
