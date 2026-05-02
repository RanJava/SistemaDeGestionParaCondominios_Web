using CondoAdmin.Application.DTO.MaintenanceRequest.CreateMaintenance;
using CondoAdmin.Application.DTO.MaintenanceRequest.ListMaintenance;
using CondoAdmin.Application.DTO.MaintenanceRequest.UpdateMaintenance;
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

        // GET: api/maintenancerequest
        [HttpGet]
        public async Task<ActionResult<ICollection<ListMaintenanceOutput>>> GetRequests()
        {
            var requests = await _contexto.MaintenanceRequests
                .AsNoTracking()
                .Select(r => new ListMaintenanceOutput
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    UnitNumber = r.Unit.UnitNumber,
                    CreatedAt = r.CreatedAt,
                    ResolvedAt = r.ResolvedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        // GET: api/maintenancerequest/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ListMaintenanceOutput>> GetRequest(int id)
        {
            var request = await _contexto.MaintenanceRequests
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new ListMaintenanceOutput
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    UnitNumber = r.Unit.UnitNumber,
                    CreatedAt = r.CreatedAt,
                    ResolvedAt = r.ResolvedAt
                })
                .FirstOrDefaultAsync();

            if (request == null)
                return NotFound();

            return Ok(request);
        }

        // GET: api/maintenancerequest/pending
        [HttpGet("pending")]
        public async Task<ActionResult<ICollection<ListMaintenanceOutput>>> GetPendingRequests()
        {
            var requests = await _contexto.MaintenanceRequests
                .AsNoTracking()
                .Where(r => r.ResolvedAt == null)
                .Select(r => new ListMaintenanceOutput
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    UnitNumber = r.Unit.UnitNumber,
                    CreatedAt = r.CreatedAt,
                    ResolvedAt = r.ResolvedAt
                })
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
            if (!requests.Any()) return Ok("No hay pendientes");

            return Ok(requests);
        }

        // POST: api/maintenancerequest
        [HttpPost]
        public async Task<ActionResult<CreateMaintenanceOutput>> CreateRequest([FromBody] CreateMaintenanceInput input)
        {
            var unit = await _contexto.Units.FindAsync(input.UnitId);
            if (unit == null)
                return NotFound($"No se encontró la unidad con ID {input.UnitId}.");

            var request = new MaintenanceRequest
            {
                Title = input.Title,
                Description = input.Description,
                UnitId = input.UnitId,
                CreatedAt = DateTime.Now,
                ResolvedAt = null
            };

            _contexto.MaintenanceRequests.Add(request);
            await _contexto.SaveChangesAsync();

            var output = new CreateMaintenanceOutput
            {
                Id = request.Id,
                Title = request.Title,
                UnitNumber = unit.UnitNumber,
                CreatedAt = request.CreatedAt
            };

            return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, output);
        }

        // PUT: api/maintenancerequest/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequest(int id, [FromBody] UpdateMaintenanceInput input)
        {
            var existing = await _contexto.MaintenanceRequests.FindAsync(id);
            if (existing == null)
                return NotFound();

            var unit = await _contexto.Units.FindAsync(input.UnitId);
            if (unit == null)
                return NotFound($"No se encontró la unidad con ID {input.UnitId}.");

            existing.Title = input.Title;
            existing.Description = input.Description;
            existing.UnitId = input.UnitId;

            await _contexto.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/maintenancerequest/{id}/resolve
        [HttpPut("{id}/resolve")]
        public async Task<ActionResult<ListMaintenanceOutput>> ResolveRequest(int id)
        {
            var existing = await _contexto.MaintenanceRequests
                .Include(r => r.Unit)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existing == null)
                return NotFound();

            if (existing.ResolvedAt != null)
                return BadRequest("La solicitud ya fue resuelta.");

            existing.ResolvedAt = DateTime.Now;
            await _contexto.SaveChangesAsync();

            return Ok(new ListMaintenanceOutput
            {
                Id = existing.Id,
                Title = existing.Title,
                Description = existing.Description,
                UnitNumber = existing.Unit.UnitNumber,
                CreatedAt = existing.CreatedAt,
                ResolvedAt = existing.ResolvedAt
            });
        }
    }
}