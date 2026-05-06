// Mappings/BuildingProfile.cs
using AutoMapper;
using CondoAdmin.Application.DTO.Building.AddBuilding;
using CondoAdmin.Application.DTO.Building.ListBuilding;
using CondoAdmin.Domain.Entities;

namespace CondoAdmin.Application.Mappings;

public class BuildingProfile : Profile
{
    public BuildingProfile()
    {
        CreateMap<Building, ListBuildingOutput>();
        CreateMap<Building, AddBuildingOutput>();
    }
}