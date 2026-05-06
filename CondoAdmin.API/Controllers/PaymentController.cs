using AutoMapper;
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
        private readonly IMapper _mapper;

        public PaymentController(AppDbContext contexto, IMapper mapper)
        {
            _contexto = contexto;
            _mapper = mapper;
        }

        // GET: api/payment
        [HttpGet]
        public async Task<ActionResult<ICollection<ListPaymentOutput>>> GetPayments()
        {
            var payments = await _contexto.Payments
                .Include(p => p.Resident)
                .AsNoTracking()
                .ToListAsync();

            return Ok(_mapper.Map<ICollection<ListPaymentOutput>>(payments));
        }

        // GET: api/payment/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ListPaymentOutput>> GetPayment(int id)
        {
            var payment = await _contexto.Payments
                .Include(p => p.Resident)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound();

            return Ok(_mapper.Map<ListPaymentOutput>(payment));
        }

        // GET: api/payment/by-resident
        [HttpGet("by-resident")]
        public async Task<ActionResult<ICollection<ListPaymentOutput>>> GetPaymentsByResident([FromQuery] int residentId)
        {
            var exists = await _contexto.Residents.AnyAsync(r => r.Id == residentId);
            if (!exists)
                return NotFound($"No se encontró el residente con ID {residentId}.");

            var payments = await _contexto.Payments
                .Include(p => p.Resident)
                .AsNoTracking()
                .Where(p => p.ResidentId == residentId)
                .ToListAsync();

            return Ok(_mapper.Map<ICollection<ListPaymentOutput>>(payments));
        }

        // GET: api/payment/pending
        [HttpGet("pending")]
        public async Task<ActionResult<ICollection<ListPaymentOutput>>> GetPendingPayments()
        {
            var today = DateTime.Today;

            var payments = await _contexto.Payments
                .Include(p => p.Resident)
                .AsNoTracking()
                .Where(p => p.PaidAt == null && p.DueDate < today)
                .OrderBy(p => p.DueDate)
                .ToListAsync();

            return Ok(_mapper.Map<ICollection<ListPaymentOutput>>(payments));
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

            payment.Resident = resident;
            var output = _mapper.Map<CreatePaymentOutput>(payment);
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
            var query = _contexto.Payments.Include(p => p.Resident).AsQueryable();

            if (residentId.HasValue)
                query = query.Where(p => p.ResidentId == residentId.Value);

            if (isPaid.HasValue)
                query = isPaid.Value
                    ? query.Where(p => p.PaidAt != null)
                    : query.Where(p => p.PaidAt == null);

            if (!string.IsNullOrWhiteSpace(month))
                query = query.Where(p => p.Month.Contains(month));

            var payments = await query.AsNoTracking().ToListAsync();
            return Ok(_mapper.Map<ICollection<ListPaymentOutput>>(payments));
        }
    }
}