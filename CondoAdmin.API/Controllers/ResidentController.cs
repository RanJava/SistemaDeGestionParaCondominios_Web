using CondoAdmin.Application.DTO.Residents.CreateResident;
using CondoAdmin.Application.DTO.Residents.ListResident;
using CondoAdmin.Application.DTO.Residents.ListResidents;
using CondoAdmin.Application.DTO.Residents.ListResidentsDebtor;
using CondoAdmin.Application.DTO.Residents.UpdateResidentInput;
using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers
{
    public class ResidentController : BaseApiController
    {
        private readonly AppDbContext _context;

        public ResidentController(AppDbContext context)
        {
            _context = context;
        }

        // GET
        [HttpGet]
        public async Task<ActionResult<ICollection<ListResidentsOutput>>> GetResidents()
        {
            var residents = await _context.Residents
                .Select(r => new ListResidentsOutput
                {
                    Id = r.Id,
                    FullName = $"{r.FirstName} {r.LastName}",
                    DNI = r.DNI,
                    Phone = r.Phone,
                    Email = r.Email,
                    UnitNumber = r.Unit != null ? r.Unit.UnitNumber : "Sin unidad"
                })
                .ToListAsync();

            return Ok(residents);
        }

        // GET id
        [HttpGet("{id}")]
        public async Task<ActionResult<ListResidentsOutput>> GetResident(int id)
        {
            var resident = await _context.Residents
                .Where(r => r.Id == id)
                .Select(r => new ListResidentsOutput
                {
                    Id = r.Id,
                    FullName = $"{r.FirstName} {r.LastName}",
                    DNI = r.DNI,
                    Phone = r.Phone,
                    Email = r.Email,
                    UnitNumber = r.Unit != null ? r.Unit.UnitNumber : "Sin unidad"
                })
                .FirstOrDefaultAsync();

            if (resident == null)
                return NotFound();

            return Ok(resident);
        }

        [HttpPost]
        public async Task<ActionResult<CreateResidentOutput>> CreateResident([FromBody] CreateResidentInput input)
        {
            var existeDNI = await _context.Residents.AnyAsync(r => r.DNI == input.DNI);
            if (existeDNI)
                return Conflict($"Ya existe un residente con el DNI {input.DNI}.");

            var resident = new Resident
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                Email = input.Email,
                Phone = input.Phone,
                DNI = input.DNI,
                MoveInDate = input.MoveInDate,
                UnitId = input.UnitId,
                IsActive = true
            };

            _context.Residents.Add(resident);
            await _context.SaveChangesAsync();

            var unitNumber = "Sin unidad";
            if (resident.UnitId != null)
            {
                var unit = await _context.Units.FindAsync(resident.UnitId);
                unitNumber = unit?.UnitNumber ?? "Sin unidad";
            }

            var output = new CreateResidentOutput
            {
                Id = resident.Id,
                FullName = $"{resident.FirstName} {resident.LastName}",
                DNI = resident.DNI,
                UnitNumber = unitNumber,
                MoveInDate = resident.MoveInDate
            };

            return CreatedAtAction(nameof(GetResident), new { id = resident.Id }, output);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResident(int id, [FromBody] UpdateResidentInput input)
        {
            var existing = await _context.Residents.FindAsync(id);
            if (existing == null)
                return NotFound();

            var existeDNI = await _context.Residents
                .AnyAsync(r => r.DNI == input.DNI && r.Id != id);
            if (existeDNI)
                return Conflict($"Ya existe otro residente con el DNI {input.DNI}.");

            existing.FirstName = input.FirstName;
            existing.LastName = input.LastName;
            existing.Email = input.Email;
            existing.Phone = input.Phone;
            existing.DNI = input.DNI;
            existing.IsActive = input.IsActive;
            existing.UnitId = input.UnitId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("by-building")]
        public async Task<ActionResult<ICollection<ListResidentByBuildingsOutput>>> GetResidentsByBuilding([FromQuery] int buildingId)
        {
            var residents = await _context.Residents
                .Where(r => r.Unit != null && r.Unit.BuildingId == buildingId)
                .Select(r => new ListResidentByBuildingsOutput
                {
                    Id = r.Id,
                    FullName = $"{r.FirstName} {r.LastName}",
                    DNI = r.DNI,
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
                .Where(r => r.Payments.Any(p => p.PaidAt == null && p.DueDate < today))
                .Select(r => new ListResidentsDebtorOutput
                {
                    Id = r.Id,
                    FullName = $"{r.FirstName} {r.LastName}",
                    DNI = r.DNI,
                    UnitNumber = r.Unit != null ? r.Unit.UnitNumber : "Sin unidad",
                    PendingPayments = r.Payments.Count(p => p.PaidAt == null && p.DueDate < today),
                    TotalDebt = r.Payments
                        .Where(p => p.PaidAt == null && p.DueDate < today)
                        .Sum(p => p.Amount)
                })
                .OrderByDescending(r => r.TotalDebt)
                .ToListAsync();

            if (!debtors.Any())
                return NotFound("No se encontraron residentes morosos.");

            return Ok(debtors);
        }

        // GET: api/resident/search
        [HttpGet("search")]
        public async Task<ActionResult<ICollection<ListResidentsOutput>>> SearchResidents(
            [FromQuery] string? name,
            [FromQuery] string? dni,
            [FromQuery] bool? isActive)
        {
            var query = _context.Residents.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(r => (r.FirstName + " " + r.LastName).Contains(name));

            if (!string.IsNullOrWhiteSpace(dni))
                query = query.Where(r => r.DNI.Contains(dni));

            if (isActive.HasValue)
                query = query.Where(r => r.IsActive == isActive.Value);

            var residents = await query
                .Select(r => new ListResidentsOutput
                {
                    Id = r.Id,
                    FullName = $"{r.FirstName} {r.LastName}",
                    DNI = r.DNI,
                    Phone = r.Phone,
                    Email = r.Email,
                    UnitNumber = r.Unit != null ? r.Unit.UnitNumber : "Sin unidad"
                })
                .ToListAsync();

            return Ok(residents);
        }
    }
}