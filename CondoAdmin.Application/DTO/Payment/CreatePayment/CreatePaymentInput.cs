using System;

namespace CondoAdmin.Application.DTO.Payment.CreatePayment;

public class CreatePaymentInput
{
    public int ResidentId { get; set; }
    public required string Month { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
}
