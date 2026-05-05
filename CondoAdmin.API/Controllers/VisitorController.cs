using AutoMapper;
using CondoAdmin.Application.DTO.Visitors.CreateVisitor;
using CondoAdmin.Application.DTO.Visitors.ListVisitor;
using CondoAdmin.Application.DTO.Visitors.UpdateVisitor;
using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers
{
    public class VisitorController : BaseApiController
    {
        private readonly AppDbContext _contexto;
        private readonly IMapper _mapper;

        public VisitorController(AppDbContext contexto, IMapper mapper)
        {
            _contexto = contexto;
            _mapper = mapper;
        }

        // GET: api/visitor
        [HttpGet]
        public async Task<ActionResult<ICollection<ListVisitorOutput>>> GetVisitors()
        {
            var visitors = await _contexto.Visitors
                .Include(v => v.Unit)
                .AsNoTracking()
                .ToListAsync();

            return Ok(_mapper.Map<ICollection<ListVisitorOutput>>(visitors));
        }

        // GET: api/visitor/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ListVisitorOutput>> GetVisitor(int id)
        {
            var visitor = await _contexto.Visitors
                .Include(v => v.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visitor == null)
                return NotFound();

            return Ok(_mapper.Map<ListVisitorOutput>(visitor));
        }

        // GET: api/visitor/inside
        [HttpGet("inside")]
        public async Task<ActionResult<ICollection<ListVisitorOutput>>> GetVisitorsInside()
        {
            var visitors = await _contexto.Visitors
                .Include(v => v.Unit)
                .AsNoTracking()
                .Where(v => v.ExitTime == null)
                .ToListAsync();

            return Ok(_mapper.Map<ICollection<ListVisitorOutput>>(visitors));
        }

        // POST: api/visitor
        [HttpPost]
        public async Task<ActionResult<CreateVisitorOutput>> CreateVisitor([FromBody] CreateVisitorInput input)
        {
            var unit = await _contexto.Units.FindAsync(input.UnitId);
            if (unit == null)
                return NotFound($"No se encontró la unidad con ID {input.UnitId}.");

            var visitor = new Visitor
            {
                FullName = input.FullName,
                DNI = input.DNI,
                LicensePlate = input.LicensePlate,
                UnitId = input.UnitId,
                EntryTime = DateTime.Now,
                ExitTime = null
            };

            _contexto.Visitors.Add(visitor);
            await _contexto.SaveChangesAsync();

            visitor.Unit = unit;
            var output = _mapper.Map<CreateVisitorOutput>(visitor);
            return CreatedAtAction(nameof(GetVisitor), new { id = visitor.Id }, output);
        }

        // PUT: api/visitor/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVisitor(int id, [FromBody] UpdateVisitorInput input)
        {
            var existing = await _contexto.Visitors.FindAsync(id);
            if (existing == null)
                return NotFound();

            var unit = await _contexto.Units.FindAsync(input.UnitId);
            if (unit == null)
                return NotFound($"No se encontró la unidad con ID {input.UnitId}.");

            existing.FullName = input.FullName;
            existing.DNI = input.DNI;
            existing.LicensePlate = input.LicensePlate;
            existing.UnitId = input.UnitId;

            await _contexto.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/visitor/{id}/exit
        [HttpPut("{id}/exit")]
        public async Task<ActionResult<ListVisitorOutput>> RegisterExit(int id)
        {
            var existing = await _contexto.Visitors
                .Include(v => v.Unit)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (existing == null)
                return NotFound();

            if (existing.ExitTime != null)
                return BadRequest("El visitante ya registró su salida.");

            existing.ExitTime = DateTime.Now;
            await _contexto.SaveChangesAsync();

            return Ok(_mapper.Map<ListVisitorOutput>(existing));
        }

        // GET: api/visitor/filter
        [HttpGet("filter")]
        public async Task<ActionResult<ICollection<ListVisitorOutput>>> FilterVisitors(
        [FromQuery] int? unitId,
        [FromQuery] bool? isInside)
        {
            var query = _contexto.Visitors.Include(v => v.Unit).AsQueryable();

            if (unitId.HasValue)
                query = query.Where(v => v.UnitId == unitId.Value);

            if (isInside.HasValue)
                query = isInside.Value
                    ? query.Where(v => v.ExitTime == null)
                    : query.Where(v => v.ExitTime != null);

            var visitors = await query.AsNoTracking().ToListAsync();
            return Ok(_mapper.Map<ICollection<ListVisitorOutput>>(visitors));
        }
    }
}