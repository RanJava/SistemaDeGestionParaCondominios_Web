namespace CondoAdmin.Application.DTO.Sale.TriggerSale;

public class TriggerSaleInput
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string DNI { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public required string MethodOfPayment { get; set; }
    public List<UnitsOfInputs> Details { get; set; } = new();
}

public class UnitsOfInputs
{
    public required string UnitNumber { get; set; }
    public string? Notes { get; set; }
    public required int Price { get; set; }  
    public required string NameBuilding {get; set;}

}