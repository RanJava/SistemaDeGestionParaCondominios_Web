namespace CondoAdmin.Application.DTO.Sale.AgregarSale;

public class AgregarSaleInput
{
    public required string DNI             { get; set; }
    public string?         FirstName       { get; set; }
    public string?         LastName        { get; set; }
    public string?         Email           { get; set; }
    public string?         Phone           { get; set; }
    public required string UnitNumber      { get; set; }
    public decimal         SalePrice       { get; set; }
    public required string MethodOfPayment { get; set; }
    public string?         Notes           { get; set; }
}