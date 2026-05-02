using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceRequestController : ControllerBase
    {
        private readonly AppDbContext _contexto;

        public MaintenanceRequestController(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        // GET
        [HttpGet]
        public async Task<ActionResult<ICollection<MaintenanceRequest>>> GetRequests()
        {
            var requests = await _contexto.MaintenanceRequests
                .Include(r => r.Unit)
                .ToListAsync();

            return Ok(requests);
        }

        // GET by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceRequest>> GetRequest(int id)
        {
            var request = await _contexto.MaintenanceRequests
                .Include(r => r.Unit)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            return Ok(request);
        }

        // POST
        [HttpPost]
        public async Task<ActionResult<MaintenanceRequest>> CreateRequest([FromBody] MaintenanceRequest request)
        {
            _contexto.MaintenanceRequests.Add(request);
            await _contexto.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, request);
        }

        // PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequest(int id, [FromBody] MaintenanceRequest request)
        {
            if (id != request.Id)
                return BadRequest("El ID no coincide con la solicitud enviada.");

            var existing = await _contexto.MaintenanceRequests.FindAsync(id);
            if (existing == null)
                return NotFound();

            // PROPIEDADES CORRECTAS
            existing.Title = request.Title;
            existing.Description = request.Description;
            existing.CreatedAt = request.CreatedAt;
            existing.ResolvedAt = request.ResolvedAt;
            existing.UnitId = request.UnitId;

            await _contexto.SaveChangesAsync();
            return NoContent();
        }
    }
}