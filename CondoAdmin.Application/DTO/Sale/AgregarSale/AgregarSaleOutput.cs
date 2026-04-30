namespace CondoAdmin.Application.DTO.Sale.AgregarSale;

public class AgregarSaleOutput
{
    public int      Id              { get; set; }
    public DateTime SaleDate        { get; set; }
    public decimal  SalePrice       { get; set; }
    public string   MethodOfPayment { get; set; } = string.Empty;
    public string?  Notes           { get; set; }
    public string   BuyerName       { get; set; } = string.Empty;
    public string   BuyerDNI        { get; set; } = string.Empty;
    public string   UnitNumber      { get; set; } = string.Empty;
    public string   BuildingName    { get; set; } = string.Empty;
}