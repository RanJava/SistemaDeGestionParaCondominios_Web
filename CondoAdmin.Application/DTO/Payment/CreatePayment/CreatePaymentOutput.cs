using System;

namespace CondoAdmin.Application.DTO.Payment.CreatePayment;

public class CreatePaymentOutput
{
    public int Id { get; set; }
    public string? ResidentName { get; set; }
    public string? Month { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
}
