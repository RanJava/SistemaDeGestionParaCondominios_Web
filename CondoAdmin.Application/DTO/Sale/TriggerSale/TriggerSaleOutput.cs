namespace CondoAdmin.Application.DTO.Sale.TriggerSale;

public class TriggerSaleOutput
{
    public string Buyer { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public string MethodOfPayment { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal Total { get; set; }
    public List<SaleDetailOutput> Units { get; set; } = new();
}

public class SaleDetailOutput
{
    public int SaleId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal SalePrice { get; set; }
    public string? Notes { get; set; }
}