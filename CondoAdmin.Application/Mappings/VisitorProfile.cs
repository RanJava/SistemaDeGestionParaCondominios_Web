// Mappings/VisitorProfile.cs
using AutoMapper;
using CondoAdmin.Application.DTO.Visitors.CreateVisitor;
using CondoAdmin.Application.DTO.Visitors.ListVisitor;
using CondoAdmin.Domain.Entities;

namespace CondoAdmin.Application.Mappings;

public class VisitorProfile : Profile
{
    public VisitorProfile()
    {
        CreateMap<Visitor, ListVisitorOutput>()
            .ForMember(d => d.UnitNumber, o => o.MapFrom(s => s.Unit.UnitNumber));

        CreateMap<Visitor, CreateVisitorOutput>()
            .ForMember(d => d.UnitNumber, o => o.MapFrom(s => s.Unit.UnitNumber));
    }
}