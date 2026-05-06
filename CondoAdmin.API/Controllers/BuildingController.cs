using AutoMapper;
using CondoAdmin.Application.DTO.Building.AddBuilding;
using CondoAdmin.Application.DTO.Building.ListBuilding;
using CondoAdmin.Application.DTO.Building.UpdateBuilding;
using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers
{
    public class BuildingController : BaseApiController
    {
        private readonly AppDbContext _contexto;
        private readonly IMapper _mapper;

        public BuildingController(AppDbContext contexto, IMapper mapper)
        {
            _contexto = contexto;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ICollection<ListBuildingOutput>>> GetBuildings()
        {
            var buildings = await _contexto.Buildings
                .AsNoTracking()
                .ToListAsync();

            return Ok(_mapper.Map<ICollection<ListBuildingOutput>>(buildings));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ListBuildingOutput>> GetBuilding(int id)
        {
            var building = await _contexto.Buildings
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (building == null)
                return NotFound();

            return Ok(_mapper.Map<ListBuildingOutput>(building));
        }

        [HttpPost]
        public async Task<ActionResult<AddBuildingOutput>> CreateBuilding([FromBody] AddBuildingInput input)
        {
            var building = new Building
            {
                Name = input.Name,
                Address = input.Address,
                City = input.City,
                TotalUnits = input.TotalUnits,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _contexto.Buildings.Add(building);
            await _contexto.SaveChangesAsync();

            var output = _mapper.Map<AddBuildingOutput>(building);
            return CreatedAtAction(nameof(GetBuilding), new { id = building.Id }, output);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuilding(int id, [FromBody] UpdateBuildingInput input)
        {
            var existing = await _contexto.Buildings.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Name = input.Name;
            existing.Address = input.Address;
            existing.City = input.City;
            existing.TotalUnits = input.TotalUnits;
            existing.IsActive = input.IsActive;

            await _contexto.SaveChangesAsync();
            return NoContent();
        }
    }
}