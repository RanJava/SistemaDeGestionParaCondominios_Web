using System;

namespace CondoAdmin.Application.DTO.Payment.ListPayment;

public class ListPaymentOutput
{
    public int Id { get; set; }
    public string? ResidentName { get; set; }
    public string? Month { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public bool IsPaid => PaidAt != null;
}
