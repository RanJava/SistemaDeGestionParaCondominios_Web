using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ResidentBuilding.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResidentController : ControllerBase
    {
        private readonly AppDbContext _contexto;

        //Constructor
        public ResidentController(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        // GET: api/clientes
        [HttpGet]
        public async Task<ActionResult<ICollection<Resident>>> GetResident()
        {
            var residents = await _contexto.Residents.ToListAsync();
            return Ok(residents);
        }

        // GET: api/clientes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Resident>> GetResident(int id)
        {
            var resident = await _contexto.Residents.FindAsync(id);

            if (resident == null)
                return NotFound();

            return Ok(resident);
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<ActionResult<Resident>> CreateResident([FromBody] Resident cliente)
        {
            _contexto.Residents.Add(cliente);
            await _contexto.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResident), new { id = cliente.Id }, cliente);
        }

        // PUT: api/clientes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResident(int id, [FromBody] Resident resident)
        {
            if (id != resident.Id)
                return BadRequest("El ID no coincide con del residente enviado.");

            var existing = await _contexto.Residents.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Actualizar propiedades
            existing.FirstName = resident.FirstName;
            existing.LastName = resident.LastName;
            existing.Email = resident.Email;
            existing.Phone = resident.Phone;
            existing.DNI = resident.DNI;
            existing.IsActive = resident.IsActive;

            await _contexto.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/clientes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResident(int id)
        {
            var cliente = await _contexto.Residents.FindAsync(id);
            if (cliente == null)
                return NotFound();

            _contexto.Residents.Remove(cliente);
            await _contexto.SaveChangesAsync();
            return NoContent();
        }
    }
}
