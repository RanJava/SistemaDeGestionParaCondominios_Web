using System;

namespace CondoAdmin.Application.DTO.Residents.ListResidentsDebtor;

public class ListResidentsDebtorOutput
{
    public int Id {get; set;}
    public required string FullName {get; set;}
    public required string DNI {get; set;}
    public required string UnitNumber {get; set;}
    public int PendingPayments {get; set;} 
    public decimal TotalDebt {get; set;} 
}
