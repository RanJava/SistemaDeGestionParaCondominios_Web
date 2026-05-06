// Mappings/UnitProfile.cs
using AutoMapper;
using CondoAdmin.Application.DTO.Unit.CreateUnit;
using CondoAdmin.Application.DTO.Unit.ListUnit;
using CondoAdmin.Domain.Entities;

namespace CondoAdmin.Application.Mappings;

public class UnitProfile : Profile
{
    public UnitProfile()
    {
        CreateMap<Unit, ListUnitOutput>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.BuildingName, o => o.MapFrom(s => s.Building.Name));

        CreateMap<Unit, CreateUnitOutput>()
            .ForMember(d => d.BuildingName, o => o.MapFrom(s => s.Building.Name));
    }
}