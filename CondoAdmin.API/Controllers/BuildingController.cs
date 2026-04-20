using CondoAdmin.Application.DTO.Building.AddBuilding;
using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CondoAdmin.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildingController : ControllerBase
    {
        private readonly AppDbContext _contexto;

        //Constructor
        public BuildingController(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        // GET: api/clientes
        [HttpGet]
        public async Task<ActionResult<ICollection<Building>>> GetBuildings()
        {
            var buildings = await _contexto.Buildings.ToListAsync();
            return Ok(buildings);
        }

        // GET: api/clientes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Building>> GetBuildings(int id)
        {
            var building = await _contexto.Buildings.FindAsync(id);

            if (building == null)
                return NotFound();

            return Ok(building);
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<ActionResult<AddBuildingOutput>> CreateBuildings([FromBody] AddBuildingInput building)
        {
            var input = new Building
            {
                Name = building.Name,
                Address = building.Address,
                City = building.City,
            };
            input.CreatedAt = DateTime.Now;
            input.IsActive = true;

            _contexto.Buildings.Add(input);
            await _contexto.SaveChangesAsync();

            var output = new AddBuildingOutput
            {
                Id = input.Id,
                Name = input.Name,
                Address = input.Address,
                City = input.City,
                TotalUnits = input.TotalUnits,
            };
            return CreatedAtAction(nameof(GetBuildings), new { id = building.Id }, output);
        }

        // PUT: api/clientes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuildings(int id, [FromBody] Building building)
        {
            if (id != building.Id)
                return BadRequest("El ID no coincide con el edificio enviado.");

            var existing = await _contexto.Buildings.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Actualizar propiedades
            existing.Name = building.Name;
            existing.Address = building.Address;
            existing.City = building.City;
            existing.TotalUnits = building.TotalUnits;
            existing.CreatedAt = building.CreatedAt;
            existing.IsActive = building.IsActive;


            await _contexto.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/clientes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            var building = await _contexto.Buildings.FindAsync(id);
            if (building == null)
                return NotFound();

            _contexto.Buildings.Remove(building);
            await _contexto.SaveChangesAsync();
            return NoContent();
        }
    }
}
