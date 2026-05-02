using System;

namespace CondoAdmin.Application.DTO.Residents.CreateResident;

public class CreateResidentInput
{
    public required string FirstName {get; set;}
    public required string LastName {get; set;}
    public required string Email {get; set;}
    public required string Phone {get; set;}
    public required string DNI {get; set;}
    public DateTime MoveInDate {get; set;}
    public int UnitId {get; set;}
}
