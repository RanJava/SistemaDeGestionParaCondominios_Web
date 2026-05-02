using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers
{
    public class UnitController : BaseApiController
    {
        private readonly AppDbContext _contexto;

        public UnitController(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        // GET: api/unit
        [HttpGet]
        public async Task<ActionResult<ICollection<Unit>>> GetUnits()
        {
            var units = await _contexto.Units
                .Include(u => u.Building)
                .ToListAsync();
            return Ok(units);
        }

        // GET: api/unit/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Unit>> GetUnit(int id)
        {
            var unit = await _contexto.Units
                .Include(u => u.Building)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null)
                return NotFound();

            return Ok(unit);
        }

        // POST: api/unit
        [HttpPost]
        public async Task<ActionResult<Unit>> CreateUnit([FromBody] Unit unit)
        {
            _contexto.Units.Add(unit);
            await _contexto.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUnit), new { id = unit.Id }, unit);
        }

        // PUT: api/unit/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUnit(int id, [FromBody] Unit unit)
        {
            if (id != unit.Id)
                return BadRequest("El ID no coincide con la unidad enviada.");

            var existing = await _contexto.Units.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.UnitNumber  = unit.UnitNumber;
            existing.Floor       = unit.Floor;
            existing.AreaM2      = unit.AreaM2;
            existing.MonthlyFee  = unit.MonthlyFee;
            existing.Status      = unit.Status;
            existing.BuildingId  = unit.BuildingId;

            await _contexto.SaveChangesAsync();
            return NoContent();
        }
    }
}
