using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _contexto;

        public PaymentController(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        // GET
        [HttpGet]
        public async Task<ActionResult<ICollection<Payment>>> GetPayments()
        {
            var payments = await _contexto.Payments
                .Include(p => p.Resident)
                .ToListAsync();

            return Ok(payments);
        }

        // GET by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _contexto.Payments
                .Include(p => p.Resident)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        // POST
        [HttpPost]
        public async Task<ActionResult<Payment>> CreatePayment([FromBody] Payment payment)
        {
            _contexto.Payments.Add(payment);
            await _contexto.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }

    }
}