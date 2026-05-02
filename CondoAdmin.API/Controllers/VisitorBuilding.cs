using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers
{
    public class VisitorController : BaseApiController
    {
        private readonly AppDbContext _contexto;

        public VisitorController(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        // GET: api/visitor
        [HttpGet]
        public async Task<ActionResult<ICollection<Visitor>>> GetVisitors()
        {
            var visitors = await _contexto.Visitors
                .Include(v => v.Unit)
                .ToListAsync();
            return Ok(visitors);
        }

        // GET: api/visitor/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Visitor>> GetVisitor(int id)
        {
            var visitor = await _contexto.Visitors
                .Include(v => v.Unit)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visitor == null)
                return NotFound();

            return Ok(visitor);
        }

        // POST: api/visitor
        [HttpPost]
        public async Task<ActionResult<Visitor>> CreateVisitor([FromBody] Visitor visitor)
        {
            _contexto.Visitors.Add(visitor);
            await _contexto.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVisitor), new { id = visitor.Id }, visitor);
        }

        // PUT: api/visitor/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVisitor(int id, [FromBody] Visitor visitor)
        {
            if (id != visitor.Id)
                return BadRequest("El ID no coincide con el visitante enviado.");

            var existing = await _contexto.Visitors.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.FullName     = visitor.FullName;
            existing.DNI          = visitor.DNI;
            existing.LicensePlate = visitor.LicensePlate;
            existing.EntryTime    = visitor.EntryTime;
            existing.ExitTime     = visitor.ExitTime;
            existing.UnitId       = visitor.UnitId;

            await _contexto.SaveChangesAsync();
            return NoContent();
        }
    }
}
