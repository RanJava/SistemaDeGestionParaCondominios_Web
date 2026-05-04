using CondoAdmin.Application.DTO.Payment.CreatePayment;
using CondoAdmin.Application.DTO.Payment.ListPayment;
using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers
{
    public class PaymentController : BaseApiController
    {
        private readonly AppDbContext _contexto;

        public PaymentController(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        // GET: api/payment
        [HttpGet]
        public async Task<ActionResult<ICollection<ListPaymentOutput>>> GetPayments()
        {
            var payments = await _contexto.Payments
                .AsNoTracking()
                .Select(p => new ListPaymentOutput
                {
                    Id = p.Id,
                    ResidentName = $"{p.Resident.FirstName} {p.Resident.LastName}",
                    Month = p.Month,
                    Amount = p.Amount,
                    DueDate = p.DueDate,
                    PaidAt = p.PaidAt
                })
                .ToListAsync();

            return Ok(payments);
        }

        // GET: api/payment/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ListPaymentOutput>> GetPayment(int id)
        {
            var payment = await _contexto.Payments
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ListPaymentOutput
                {
                    Id = p.Id,
                    ResidentName = $"{p.Resident.FirstName} {p.Resident.LastName}",
                    Month = p.Month,
                    Amount = p.Amount,
                    DueDate = p.DueDate,
                    PaidAt = p.PaidAt
                })
                .FirstOrDefaultAsync();

            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        // GET: api/payment/by-resident
        [HttpGet("by-resident")]
        public async Task<ActionResult<ICollection<ListPaymentOutput>>> GetPaymentsByResident([FromQuery] int residentId)
        {
            var exists = await _contexto.Residents.AnyAsync(r => r.Id == residentId);
            if (!exists)
                return NotFound($"No se encontró el residente con ID {residentId}.");

            var payments = await _contexto.Payments
                .AsNoTracking()
                .Where(p => p.ResidentId == residentId)
                .Select(p => new ListPaymentOutput
                {
                    Id = p.Id,
                    ResidentName = $"{p.Resident.FirstName} {p.Resident.LastName}",
                    Month = p.Month,
                    Amount = p.Amount,
                    DueDate = p.DueDate,
                    PaidAt = p.PaidAt
                })
                .ToListAsync();

            return Ok(payments);
        }

        // GET: api/payment/pending
        [HttpGet("pending")]
        public async Task<ActionResult<ICollection<ListPaymentOutput>>> GetPendingPayments()
        {
            var today = DateTime.Today;

            var payments = await _contexto.Payments
                .AsNoTracking()
                .Where(p => p.PaidAt == null && p.DueDate < today)
                .Select(p => new ListPaymentOutput
                {
                    Id = p.Id,
                    ResidentName = $"{p.Resident.FirstName} {p.Resident.LastName}",
                    Month = p.Month,
                    Amount = p.Amount,
                    DueDate = p.DueDate,
                    PaidAt = p.PaidAt
                })
                .OrderBy(p => p.DueDate)
                .ToListAsync();

            return Ok(payments);
        }

        // POST: api/payment
        [HttpPost]
        public async Task<ActionResult<CreatePaymentOutput>> CreatePayment([FromBody] CreatePaymentInput input)
        {
            var resident = await _contexto.Residents.FindAsync(input.ResidentId);
            if (resident == null)
                return NotFound($"No se encontró el residente con ID {input.ResidentId}.");

            var payment = new Payment
            {
                ResidentId = input.ResidentId,
                Month = input.Month,
                Amount = input.Amount,
                DueDate = DateTime.UtcNow,
                Notes = input.Notes,
                PaidAt = null,
            };

            _contexto.Payments.Add(payment);
            await _contexto.SaveChangesAsync();

            var output = new CreatePaymentOutput
            {
                Id = payment.Id,
                ResidentName = $"{resident.FirstName} {resident.LastName}",
                Month = payment.Month,
                Amount = payment.Amount,
                DueDate = payment.DueDate
            };

            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, output);
        }

        [HttpPatch("{id}/pay")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var payment = await _contexto.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            payment.PaidAt = DateTime.UtcNow; 
            await _contexto.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/payment/filter
        [HttpGet("filter")]
        public async Task<ActionResult<ICollection<ListPaymentOutput>>> FilterPayments(
            [FromQuery] int? residentId,
            [FromQuery] bool? isPaid,
            [FromQuery] string? month)
        {
            var query = _contexto.Payments.AsQueryable();

            if (residentId.HasValue)
                query = query.Where(p => p.ResidentId == residentId.Value);

            if (isPaid.HasValue)
                query = isPaid.Value
                    ? query.Where(p => p.PaidAt != null)
                    : query.Where(p => p.PaidAt == null);

            if (!string.IsNullOrWhiteSpace(month))
                query = query.Where(p => p.Month.Contains(month));

            var payments = await query
                .AsNoTracking()
                .Select(p => new ListPaymentOutput
                {
                    Id = p.Id,
                    ResidentName = $"{p.Resident.FirstName} {p.Resident.LastName}",
                    Month = p.Month,
                    Amount = p.Amount,
                    DueDate = p.DueDate,
                    PaidAt = p.PaidAt
                })
                .ToListAsync();

            return Ok(payments);
        }
    }
}