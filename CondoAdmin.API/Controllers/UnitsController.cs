using AutoMapper;
using CondoAdmin.Application.DTO.Unit.CreateUnit;
using CondoAdmin.Application.DTO.Unit.ListUnit;
using CondoAdmin.Application.DTO.Unit.UpdateUnit;
using CondoAdmin.Domain.Entities;
using CondoAdmin.Domain.Enums;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers
{
    public class UnitController : BaseApiController
    {
        private readonly AppDbContext _contexto;
        private readonly IMapper _mapper;

        public UnitController(AppDbContext contexto, IMapper mapper)
        {
            _contexto = contexto;
            _mapper = mapper;
        }

        // GET: api/unit
        [HttpGet]
        public async Task<ActionResult<ICollection<ListUnitOutput>>> GetUnits()
        {
            var units = await _contexto.Units
                .Include(u => u.Building)
                .AsNoTracking()
                .ToListAsync();

            return Ok(_mapper.Map<ICollection<ListUnitOutput>>(units));
        }

        // GET: api/unit/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ListUnitOutput>> GetUnit(int id)
        {
            var unit = await _contexto.Units
                .Include(u => u.Building)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null)
                return NotFound();

            return Ok(_mapper.Map<ListUnitOutput>(unit));
        }

        // GET: api/unit/by-building/{buildingId}
        [HttpGet("by-building/{buildingId}")]
        public async Task<ActionResult<ICollection<ListUnitOutput>>> GetUnitsByBuilding(int buildingId)
        {
            var exists = await _contexto.Buildings.AnyAsync(b => b.Id == buildingId);
            if (!exists)
                return NotFound($"No se encontró el edificio con ID {buildingId}.");

            var units = await _contexto.Units
                .Include(u => u.Building)
                .AsNoTracking()
                .Where(u => u.BuildingId == buildingId)
                .ToListAsync();

            return Ok(_mapper.Map<ICollection<ListUnitOutput>>(units));
        }

        // GET: api/unit/available
        [HttpGet("available")]
        public async Task<ActionResult<ICollection<ListUnitOutput>>> GetAvailableUnits()
        {
            var units = await _contexto.Units
                .Include(u => u.Building)
                .AsNoTracking()
                .Where(u => u.Status == UnitStatus.Available)
                .ToListAsync();

            return Ok(_mapper.Map<ICollection<ListUnitOutput>>(units));
        }

        // POST: api/unit
        [HttpPost]
        public async Task<ActionResult<CreateUnitOutput>> CreateUnit([FromBody] CreateUnitInput input)
        {
            var building = await _contexto.Buildings.FindAsync(input.BuildingId);
            if (building == null)
                return NotFound($"No se encontró el edificio con ID {input.BuildingId}.");

            var unit = new Unit
            {
                UnitNumber = input.UnitNumber,
                Floor = input.Floor,
                AreaM2 = input.AreaM2,
                MonthlyFee = input.MonthlyFee,
                BuildingId = input.BuildingId,
                Status = UnitStatus.Available
            };

            _contexto.Units.Add(unit);
            await _contexto.SaveChangesAsync();

            unit.Building = building;
            var output = _mapper.Map<CreateUnitOutput>(unit);
            return CreatedAtAction(nameof(GetUnit), new { id = unit.Id }, output);
        }

        // PUT: api/unit/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUnit(int id, [FromBody] UpdateUnitInput input)
        {
            var existing = await _contexto.Units.FindAsync(id);
            if (existing == null)
                return NotFound();

            var building = await _contexto.Buildings.FindAsync(input.BuildingId);
            if (building == null)
                return NotFound($"No se encontró el edificio con ID {input.BuildingId}.");

            existing.UnitNumber = input.UnitNumber;
            existing.Floor = input.Floor;
            existing.AreaM2 = input.AreaM2;
            existing.MonthlyFee = input.MonthlyFee;
            existing.BuildingId = input.BuildingId;

            await _contexto.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/unit/filter
        [HttpGet("filter")]
        public async Task<ActionResult<ICollection<ListUnitOutput>>> FilterUnits(
            [FromQuery] int? buildingId,
            [FromQuery] string? status)
        {
            var query = _contexto.Units.Include(u => u.Building).AsQueryable();

            if (buildingId.HasValue)
                query = query.Where(u => u.BuildingId == buildingId.Value);

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<UnitStatus>(status, ignoreCase: true, out var parsedStatus))
                    query = query.Where(u => u.Status == parsedStatus);
                else
                    return BadRequest($"Estado '{status}' no válido. Use: Available, Sold, Rented.");
            }

            var units = await query.AsNoTracking().ToListAsync();
            return Ok(_mapper.Map<ICollection<ListUnitOutput>>(units));
        }
    }
}