namespace CondoAdmin.Application.DTO.Sale.TriggerSale;

public class TriggerSaleOutput
{
    public string? Buyer { get; set; } 
    public string? DNI { get; set; }
    public string? MethodOfPayment { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal Total { get; set; }
    public List<SaleDetailOutput> Units { get; set; } = new();
}

public class SaleDetailOutput
{
    public int SaleId { get; set; }
    public string? UnitNumber { get; set; }
    public int Floor { get; set; }
    public decimal SalePrice { get; set; }
    public string? Notes { get; set; }

        public required string NameBuilding {get; set;}

}