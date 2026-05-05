using AutoMapper;
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
        private readonly IMapper _mapper;

        public ResidentController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ICollection<ListResidentsOutput>>> GetResidents()
        {
            var residents = await _context.Residents
                .Include(r => r.Unit)
                .ToListAsync();

            return Ok(_mapper.Map<ICollection<ListResidentsOutput>>(residents));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ListResidentsOutput>> GetResident(int id)
        {
            var resident = await _context.Residents
                .Include(r => r.Unit)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resident == null)
                return NotFound();

            return Ok(_mapper.Map<ListResidentsOutput>(resident));
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

            if (resident.UnitId != null)
                await _context.Entry(resident).Reference(r => r.Unit).LoadAsync();

            var output = _mapper.Map<CreateResidentOutput>(resident);
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
                .Include(r => r.Unit)
                .Where(r => r.Unit != null && r.Unit.BuildingId == buildingId)
                .ToListAsync();

            if (!residents.Any())
                return NotFound($"No se encontraron residentes para el edificio con ID {buildingId}.");

            return Ok(_mapper.Map<ICollection<ListResidentByBuildingsOutput>>(residents));
        }

        [HttpGet("debtors")]
        public async Task<ActionResult<ICollection<ListResidentsDebtorOutput>>> GetDebtors()
        {
            var today = DateTime.Today;

            var debtors = await _context.Residents
                .Include(r => r.Unit)
                .Include(r => r.Payments)
                .Where(r => r.Payments.Any(p => p.PaidAt == null && p.DueDate < today))
                .ToListAsync();

            if (!debtors.Any())
                return NotFound("No se encontraron residentes morosos.");

            var output = _mapper.Map<ICollection<ListResidentsDebtorOutput>>(debtors);

            foreach (var item in output)
            {
                var resident = debtors.First(r => r.Id == item.Id);
                item.PendingPayments = resident.Payments.Count(p => p.PaidAt == null && p.DueDate < today);
                item.TotalDebt = resident.Payments
                    .Where(p => p.PaidAt == null && p.DueDate < today)
                    .Sum(p => p.Amount);
            }

            return Ok(output.OrderByDescending(r => r.TotalDebt));
        }

        [HttpGet("search")]
        public async Task<ActionResult<ICollection<ListResidentsOutput>>> SearchResidents(
            [FromQuery] string? name,
            [FromQuery] string? dni,
            [FromQuery] bool? isActive)
        {
            var query = _context.Residents.Include(r => r.Unit).AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(r => (r.FirstName + " " + r.LastName).Contains(name));

            if (!string.IsNullOrWhiteSpace(dni))
                query = query.Where(r => r.DNI.Contains(dni));

            if (isActive.HasValue)
                query = query.Where(r => r.IsActive == isActive.Value);

            var residents = await query.ToListAsync();
            return Ok(_mapper.Map<ICollection<ListResidentsOutput>>(residents));
        }
    }
}