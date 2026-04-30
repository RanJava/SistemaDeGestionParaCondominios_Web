namespace CondoAdmin.Application.DTO.Sale.ActualizarSale;

public class ActualizarSaleInput
{
    public int             Id              { get; set; }
    public decimal         SalePrice       { get; set; }
    public required string MethodOfPayment { get; set; }
    public string?         Notes           { get; set; }
}