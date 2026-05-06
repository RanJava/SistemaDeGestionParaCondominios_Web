// Mappings/PaymentProfile.cs
using AutoMapper;
using CondoAdmin.Application.DTO.Payment.CreatePayment;
using CondoAdmin.Application.DTO.Payment.ListPayment;
using CondoAdmin.Domain.Entities;

namespace CondoAdmin.Application.Mappings;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<Payment, ListPaymentOutput>()
            .ForMember(d => d.ResidentName, o => o.MapFrom(s => $"{s.Resident.FirstName} {s.Resident.LastName}"));

        CreateMap<Payment, CreatePaymentOutput>()
            .ForMember(d => d.ResidentName, o => o.Ignore());
    }
}