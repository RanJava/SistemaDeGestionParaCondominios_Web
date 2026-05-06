// Mappings/MaintenanceProfile.cs
using AutoMapper;
using CondoAdmin.Application.DTO.MaintenanceRequest.CreateMaintenance;
using CondoAdmin.Application.DTO.MaintenanceRequest.ListMaintenance;
using CondoAdmin.Domain.Entities;

namespace CondoAdmin.Application.Mappings;

public class MaintenanceProfile : Profile
{
    public MaintenanceProfile()
    {
        CreateMap<MaintenanceRequest, ListMaintenanceOutput>()
            .ForMember(d => d.UnitNumber, o => o.MapFrom(s => s.Unit.UnitNumber));

        CreateMap<MaintenanceRequest, CreateMaintenanceOutput>()
            .ForMember(d => d.UnitNumber, o => o.MapFrom(s => s.Unit.UnitNumber));
    }
}