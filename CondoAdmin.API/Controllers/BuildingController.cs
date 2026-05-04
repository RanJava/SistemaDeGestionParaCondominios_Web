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

        public BuildingController(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        // GET: api/building
        [HttpGet]
        public async Task<ActionResult<ICollection<ListBuildingOutput>>> GetBuildings()
        {
            var buildings = await _contexto.Buildings
                .AsNoTracking()
                .Select(b => new ListBuildingOutput
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address = b.Address,
                    City = b.City,
                    TotalUnits = b.TotalUnits,
                    IsActive = b.IsActive
                })
                .ToListAsync();

            return Ok(buildings);
        }

        // GET: api/building/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ListBuildingOutput>> GetBuilding(int id)
        {
            var building = await _contexto.Buildings
                .AsNoTracking()
                .Where(b => b.Id == id)
                .Select(b => new ListBuildingOutput
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address = b.Address,
                    City = b.City,
                    TotalUnits = b.TotalUnits,
                    IsActive = b.IsActive
                })
                .FirstOrDefaultAsync();

            if (building == null)
                return NotFound();

            return Ok(building);
        }

        // POST: api/building
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

            var output = new AddBuildingOutput
            {
                Id = building.Id,
                Name = building.Name,
                Address = building.Address,
                City = building.City,
                TotalUnits = building.TotalUnits
            };

            return CreatedAtAction(nameof(GetBuilding), new { id = building.Id }, output);
        }

        // PUT: api/building/{id}
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