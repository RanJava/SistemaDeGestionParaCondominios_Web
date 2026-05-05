using AutoMapper;
using CondoAdmin.Application.DTO.MaintenanceRequest.CreateMaintenance;
using CondoAdmin.Application.DTO.MaintenanceRequest.ListMaintenance;
using CondoAdmin.Application.DTO.MaintenanceRequest.UpdateMaintenance;
using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers
{
    public class MaintenanceRequestController : BaseApiController
    {
        private readonly AppDbContext _contexto;
        private readonly IMapper _mapper;

        public MaintenanceRequestController(AppDbContext contexto, IMapper mapper)
        {
            _contexto = contexto;
            _mapper = mapper;
        }

        // GET: api/maintenancerequest
        [HttpGet]
        public async Task<ActionResult<ICollection<ListMaintenanceOutput>>> GetRequests()
        {
            var requests = await _contexto.MaintenanceRequests
                .Include(r => r.Unit)
                .AsNoTracking()
                .ToListAsync();

            return Ok(_mapper.Map<ICollection<ListMaintenanceOutput>>(requests));
        }

        // GET: api/maintenancerequest/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ListMaintenanceOutput>> GetRequest(int id)
        {
            var request = await _contexto.MaintenanceRequests
                .Include(r => r.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            return Ok(_mapper.Map<ListMaintenanceOutput>(request));
        }

        // GET: api/maintenancerequest/pending
        [HttpGet("pending")]
        public async Task<ActionResult<ICollection<ListMaintenanceOutput>>> GetPendingRequests()
        {
            var requests = await _contexto.MaintenanceRequests
                .Include(r => r.Unit)
                .AsNoTracking()
                .Where(r => r.ResolvedAt == null)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            return Ok(_mapper.Map<ICollection<ListMaintenanceOutput>>(requests));
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

            request.Unit = unit;
            var output = _mapper.Map<CreateMaintenanceOutput>(request);
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

            return Ok(_mapper.Map<ListMaintenanceOutput>(existing));
        }

        // GET: api/maintenancerequest/filter
        [HttpGet("filter")]
        public async Task<ActionResult<ICollection<ListMaintenanceOutput>>> FilterRequests(
            [FromQuery] int? unitId,
            [FromQuery] bool? isResolved)
        {
            var query = _contexto.MaintenanceRequests.Include(r => r.Unit).AsQueryable();

            if (unitId.HasValue)
                query = query.Where(r => r.UnitId == unitId.Value);

            if (isResolved.HasValue)
                query = isResolved.Value
                    ? query.Where(r => r.ResolvedAt != null)
                    : query.Where(r => r.ResolvedAt == null);

            var requests = await query
                .AsNoTracking()
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            return Ok(_mapper.Map<ICollection<ListMaintenanceOutput>>(requests));
        }
    }
}