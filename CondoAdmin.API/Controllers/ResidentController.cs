using CondoAdmin.Application.DTO.Residents.ListResident;
using CondoAdmin.Application.DTO.Residents.ListResidentsDebtor;
using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResidentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResidentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/resident
[HttpGet]
public async Task<ActionResult<ICollection<Resident>>> GetResidents()
{
    var residents = await _context.Residents
        .Include(r => r.Unit)
            .ThenInclude(u => u!.Building)
        .ToListAsync();
    return Ok(residents);
}

        // GET: api/resident/{id}
[HttpGet("{id}")]
public async Task<ActionResult<Resident>> GetResident(int id)
{
    var resident = await _context.Residents
        .Include(r => r.Unit)
            .ThenInclude(u => u!.Building)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (resident == null)
        return NotFound();

    return Ok(resident);
}

        [HttpPost]
        public async Task<ActionResult<Resident>> CreateResident([FromBody] Resident resident)
        {
            _context.Residents.Add(resident);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetResident), new { id = resident.Id }, resident);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResident(int id, [FromBody] Resident resident)
        {
            if (id != resident.Id)
                return BadRequest("El ID no coincide con el residente enviado.");

            var existing = await _context.Residents.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.FirstName = resident.FirstName;
            existing.LastName  = resident.LastName;
            existing.Email     = resident.Email;
            existing.Phone     = resident.Phone;
            existing.DNI       = resident.DNI;
            existing.IsActive  = resident.IsActive;
            existing.UnitId    = resident.UnitId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("by-building")]
        public async Task<ActionResult<ICollection<ListResidentOutput>>> GetResidentsByBuilding([FromQuery] int buildingId)
        {
            var residents = await _context.Residents
                .Include(r => r.Unit)
                .Where(r => r.Unit != null && r.Unit.BuildingId == buildingId)
                .Select(r => new ListResidentOutput
                {
                    Id         = r.Id,
                    FullName   = $"{r.FirstName} {r.LastName}",
                    DNI        = r.DNI,
                    UnitNumber = r.Unit!.UnitNumber
                })
                .ToListAsync();

            if (!residents.Any())
                return NotFound($"No se encontraron residentes para el edificio con ID {buildingId}.");

            return Ok(residents);
        }

        // GET: api/resident/debtors
        [HttpGet("debtors")]
        public async Task<ActionResult<ICollection<ListResidentsDebtorOutput>>> GetDebtors()
        {
            var today = DateTime.Today;

            var debtors = await _context.Residents
                .Include(r => r.Unit)
                .Include(r => r.Payments)
                .Where(r => r.Payments.Any(p => p.PaidAt == null && p.DueDate < today))
                .Select(r => new ListResidentsDebtorOutput
                {
                    Id             = r.Id,
                    FullName       = $"{r.FirstName} {r.LastName}",
                    DNI            = r.DNI,
                    UnitNumber     = r.Unit != null ? r.Unit.UnitNumber : "Sin unidad",
                    PendingPayments = r.Payments.Count(p => p.PaidAt == null && p.DueDate < today),
                    TotalDebt      = r.Payments
                                    .Where(p => p.PaidAt == null && p.DueDate < today)
                                    .Sum(p => p.Amount)
                })
                .OrderByDescending(r => r.TotalDebt)
                .ToListAsync();

            if (!debtors.Any())
                return NotFound("No se encontraron residentes morosos.");

            return Ok(debtors);
        }

    }
}