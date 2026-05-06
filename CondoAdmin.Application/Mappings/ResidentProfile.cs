// Mappings/ResidentProfile.cs
using AutoMapper;
using CondoAdmin.Application.DTO.Residents.CreateResident;
using CondoAdmin.Application.DTO.Residents.ListResident;
using CondoAdmin.Application.DTO.Residents.ListResidents;
using CondoAdmin.Application.DTO.Residents.ListResidentsDebtor;
using CondoAdmin.Domain.Entities;

namespace CondoAdmin.Application.Mappings;

public class ResidentProfile : Profile
{
    public ResidentProfile()
    {
        CreateMap<Resident, ListResidentsOutput>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.UnitNumber, o => o.MapFrom(s => s.Unit != null ? s.Unit.UnitNumber : "Sin unidad"));

        CreateMap<Resident, ListResidentByBuildingsOutput>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.UnitNumber, o => o.MapFrom(s => s.Unit!.UnitNumber));

        CreateMap<Resident, ListResidentsDebtorOutput>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.UnitNumber, o => o.MapFrom(s => s.Unit != null ? s.Unit.UnitNumber : "Sin unidad"))
            .ForMember(d => d.PendingPayments, o => o.Ignore())
            .ForMember(d => d.TotalDebt, o => o.Ignore());

        CreateMap<Resident, CreateResidentOutput>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.UnitNumber, o => o.Ignore());
    }
}