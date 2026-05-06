using System.Globalization;
using CondoAdmin.Application.DTO.Rental.GetRental;
using CondoAdmin.Application.DTO.Rental.PayRental;
using CondoAdmin.Application.DTO.Rental.RenewRental;
using CondoAdmin.Application.DTO.Rental.TriggerRental;
using CondoAdmin.Domain.Entities;
using CondoAdmin.Domain.Enums;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers;

/// <summary>
/// Gestiona el ciclo de vida completo de los contratos de alquiler:
/// creación, consulta, pagos (simples, múltiples, adelantados), renovación y terminación.
/// </summary>
public class RentalController : BaseApiController
{
    private readonly AppDbContext _context;

    public RentalController(AppDbContext context)
    {
        _context = context;
    }

    // ──────────────────────────────────────────────────────────────────
    // GET: api/rental
    // Lista todos los contratos con deuda calculada en tiempo real.
    // ──────────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<ICollection<GetRentalOutput>>> GetRentals()
    {
        var contracts = await _context.RentalContracts
            .Include(c => c.Resident)
            .Include(c => c.Unit)
                .ThenInclude(u => u.Building)
            .Include(c => c.Payments)
            .AsNoTracking()
            .ToListAsync();

        return Ok(contracts.Select(MapToOutput).ToList());
    }

    // ──────────────────────────────────────────────────────────────────
    // GET: api/rental/{id}
    // ──────────────────────────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<ActionResult<GetRentalOutput>> GetRental(int id)
    {
        var contract = await _context.RentalContracts
            .Include(c => c.Resident)
            .Include(c => c.Unit)
                .ThenInclude(u => u.Building)
            .Include(c => c.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract is null)
            return NotFound($"Contrato {id} no encontrado.");

        return Ok(MapToOutput(contract));
    }

    // ──────────────────────────────────────────────────────────────────
    // POST: api/rental/trigger
    // Crea contratos para una o varias unidades en una sola operación.
    // Si el inquilino no existe por DNI, se crea automáticamente.
    // Por cada contrato se pre-generan los pagos mensuales del período.
    // ──────────────────────────────────────────────────────────────────
    [HttpPost("trigger")]
    public async Task<ActionResult<TriggerRentalOutput>> TriggerRental([FromBody] TriggerRentalInput input)
    {
        var now = DateTime.UtcNow;

        // Buscar o crear el residente por DNI
        var resident = await _context.Residents
            .FirstOrDefaultAsync(r => r.DNI == input.DNI);

        if (resident is null)
        {
            resident = new Resident
            {
                FirstName  = input.FirstName!,
                LastName   = input.LastName!,
                DNI        = input.DNI,
                Email      = input.Email!,
                Phone      = input.Phone!,
                MoveInDate = now,
                IsActive   = true
            };
            _context.Residents.Add(resident);
            await _context.SaveChangesAsync();
        }

        var outputContracts = new List<RentalContractOutput>();

        foreach (var item in input.Units)
        {
            // FIX: Include Building para poder leer su nombre en el output
            var unit = await _context.Units
                .Include(u => u.Building)
                .FirstOrDefaultAsync(u => u.UnitNumber == item.UnitNumber.Trim());

            if (unit is null)
                return BadRequest($"La unidad '{item.UnitNumber}' no existe.");

            if (unit.Status != UnitStatus.Available)
                return BadRequest($"La unidad '{item.UnitNumber}' no está disponible (estado: {unit.Status}).");

            if (item.EndDate <= item.StartDate)
                return BadRequest($"La fecha de fin debe ser posterior a la fecha de inicio en unidad '{item.UnitNumber}'.");

            // Crear contrato
            var contract = new RentalContract
            {
                StartDate     = item.StartDate,
                EndDate       = item.EndDate,
                MonthlyRent   = item.MonthlyRent,
                DepositAmount = item.DepositAmount,
                Notes         = item.Notes,
                Status        = RentalContractStatus.Active,
                CreditBalance = 0,
                UnitId        = unit.Id,
                ResidentId    = resident.Id
            };

            _context.RentalContracts.Add(contract);
            unit.Status = UnitStatus.Rented;
            await _context.SaveChangesAsync(); // Necesitamos el ID del contrato para los pagos

            // Pre-generar pagos mensuales para todo el período del contrato
            var payments = GenerateMonthlyPayments(contract, resident.Id, item.StartDate, item.EndDate);
            _context.Payments.AddRange(payments);
            await _context.SaveChangesAsync();

            var totalMonths = payments.Count;

            outputContracts.Add(new RentalContractOutput
            {
                ContractId         = contract.Id,
                UnitNumber         = unit.UnitNumber,
                BuildingName       = unit.Building?.Name ?? "",
                StartDate          = contract.StartDate,
                EndDate            = contract.EndDate,
                MonthlyRent        = contract.MonthlyRent,
                DepositAmount      = contract.DepositAmount,
                TotalMonths        = totalMonths,
                TotalContractValue = contract.MonthlyRent * totalMonths
            });
        }

        return Ok(new TriggerRentalOutput
        {
            Tenant    = $"{resident.FirstName} {resident.LastName}",
            DNI       = resident.DNI,
            CreatedAt = now,
            Contracts = outputContracts
        });
    }

    // ──────────────────────────────────────────────────────────────────
    // POST: api/rental/{contractId}/pay
    // Procesa un pago que puede cubrir múltiples meses pendientes.
    //
    // Lógica de distribución:
    //   1. Se suma el saldo a favor existente al monto recibido.
    //   2. Se aplica al mes más antiguo pendiente primero (FIFO).
    //   3. Si sobra dinero después de cubrir todos los pendientes,
    //      el excedente queda como nuevo saldo a favor.
    // ──────────────────────────────────────────────────────────────────
    [HttpPost("{contractId}/pay")]
    public async Task<ActionResult<RentalPaymentOutput>> Pay(int contractId, [FromBody] RentalPaymentInput input)
    {
        var contract = await _context.RentalContracts
            .Include(c => c.Resident)
            .Include(c => c.Unit)
            .FirstOrDefaultAsync(c => c.Id == contractId);

        if (contract is null)
            return NotFound($"Contrato {contractId} no encontrado.");

        if (contract.Status != RentalContractStatus.Active)
            return BadRequest("Solo se pueden procesar pagos de contratos activos.");

        if (input.Amount <= 0)
            return BadRequest("El monto debe ser mayor a cero.");

        // Cargar pagos pendientes ordenados por fecha de vencimiento (más antiguo primero)
        var pendingPayments = await _context.Payments
            .Where(p => p.RentalContractId == contractId && p.PaidAt == null)
            .OrderBy(p => p.DueDate)
            .ToListAsync();

        var periodsPaid  = new List<string>();

        // Fondo disponible = saldo a favor previo + nuevo pago
        var available = contract.CreditBalance + input.Amount;
        var applied   = 0m;

        foreach (var payment in pendingPayments)
        {
            if (available >= payment.Amount)
            {
                // Hay suficiente para cubrir este mes
                payment.PaidAt = DateTime.UtcNow;
                payment.Notes  = $"{input.PaymentMethod}. {input.Notes}".Trim(' ', '.');
                available     -= payment.Amount;
                applied       += payment.Amount;
                periodsPaid.Add(payment.Month);
            }
            else
            {
                // No alcanza para el siguiente mes — lo que queda va al saldo a favor
                break;
            }
        }

        // Actualizar saldo a favor con el remanente
        contract.CreditBalance = available;
        await _context.SaveChangesAsync();

        var message = periodsPaid.Any()
            ? $"Pago aplicado a {periodsPaid.Count} período(s): {string.Join(", ", periodsPaid)}."
            : "El monto no alcanzó para cubrir el siguiente período pendiente. Se acreditó al saldo a favor.";

        if (contract.CreditBalance > 0)
            message += $" Saldo a favor: {contract.CreditBalance:C2}.";

        return Ok(new RentalPaymentOutput
        {
            ContractId       = contract.Id,
            TenantName       = $"{contract.Resident.FirstName} {contract.Resident.LastName}",
            UnitNumber       = contract.Unit.UnitNumber,
            AmountReceived   = input.Amount,
            AmountApplied    = applied,
            NewCreditBalance = contract.CreditBalance,
            PeriodsPaid      = periodsPaid,
            Message          = message
        });
    }

    // ──────────────────────────────────────────────────────────────────
    // PUT: api/rental/{id}/renew
    // Extiende la fecha de fin del contrato manteniendo el mismo precio.
    // Genera los nuevos pagos mensuales para el período extendido.
    // Si el precio debe cambiar, se debe crear un nuevo contrato.
    // ──────────────────────────────────────────────────────────────────
    [HttpPut("{id}/renew")]
    public async Task<IActionResult> RenewContract(int id, [FromBody] RenewRentalInput input)
    {
        var contract = await _context.RentalContracts
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract is null)
            return NotFound($"Contrato {id} no encontrado.");

        if (contract.Status != RentalContractStatus.Active)
            return BadRequest("Solo se pueden renovar contratos activos.");

        if (input.NewEndDate <= contract.EndDate)
            return BadRequest("La nueva fecha de fin debe ser posterior a la fecha actual de fin del contrato.");

        // Generar pagos solo para el período nuevo (desde el mes siguiente al fin actual)
        var extensionStart = contract.EndDate.AddMonths(1);
        var oldEndDate     = contract.EndDate;

        contract.EndDate = input.NewEndDate;
        if (!string.IsNullOrWhiteSpace(input.Notes))
            contract.Notes = $"{contract.Notes} | Renovado: {input.Notes}".TrimStart(' ', '|');

        var newPayments = GenerateMonthlyPayments(contract, contract.ResidentId, extensionStart, input.NewEndDate);
        _context.Payments.AddRange(newPayments);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message       = $"Contrato renovado. Período extendido desde {extensionStart:MMMM yyyy} hasta {input.NewEndDate:MMMM yyyy}.",
            NewPeriods    = newPayments.Count,
            AdditionalValue = newPayments.Count * contract.MonthlyRent
        });
    }

    // ──────────────────────────────────────────────────────────────────
    // PUT: api/rental/{id}/terminate
    // Termina el contrato manualmente.
    // La unidad vuelve a "Disponible" para ser vendida o alquilada de nuevo.
    // Es una acción deliberada del admin — no automática.
    // ──────────────────────────────────────────────────────────────────
    [HttpPut("{id}/terminate")]
    public async Task<IActionResult> TerminateContract(int id)
    {
        var contract = await _context.RentalContracts
            .Include(c => c.Unit)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract is null)
            return NotFound($"Contrato {id} no encontrado.");

        if (contract.Status != RentalContractStatus.Active)
            return BadRequest("El contrato no está activo.");

        contract.Status     = RentalContractStatus.Terminated;
        contract.Unit.Status = UnitStatus.Available;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message    = $"Contrato {id} terminado. Unidad '{contract.Unit.UnitNumber}' disponible.",
            TerminatedAt = DateTime.UtcNow
        });
    }

    // ──────────────────────────────────────────────────────────────────
    // HELPERS PRIVADOS
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Genera un Payment por cada mes del rango [start, end].
    /// El vencimiento se fija el día 5 de cada mes (práctica común en Bolivia).
    /// Se usa cultura es-ES para los nombres de mes en español.
    /// </summary>
    private static List<Payment> GenerateMonthlyPayments(
        RentalContract contract, int residentId, DateTime start, DateTime end)
    {
        var culture  = new CultureInfo("es-ES");
        var payments = new List<Payment>();

        // Normalizamos al primer día del mes para iterar correctamente
        var current = new DateTime(start.Year, start.Month, 1);
        var endNorm = new DateTime(end.Year, end.Month, 1);

        while (current <= endNorm)
        {
            payments.Add(new Payment
            {
                Month             = culture.TextInfo.ToTitleCase(current.ToString("MMMM yyyy", culture)),
                Amount            = contract.MonthlyRent,
                DueDate           = new DateTime(current.Year, current.Month, 5),
                PaidAt            = null,
                ResidentId        = residentId,
                RentalContractId  = contract.Id
            });
            current = current.AddMonths(1);
        }

        return payments;
    }

    /// <summary>
    /// Proyección de RentalContract → GetRentalOutput.
    /// Centralizada para no duplicar la lógica en GET all y GET by id.
    /// </summary>
    private static GetRentalOutput MapToOutput(RentalContract c)
{
    var today = DateTime.UtcNow;

    // Solo contamos como deuda los pagos cuya fecha ya venció
    // Los meses futuros no son deuda hasta que llegue su fecha
    var pending       = c.Payments.Where(p => p.PaidAt == null && p.DueDate <= today).ToList();
    var rawDebt       = pending.Sum(p => p.Amount);
    var effectiveDebt = Math.Max(0, rawDebt - c.CreditBalance);

    return new GetRentalOutput
    {
        Id            = c.Id,
        TenantName    = $"{c.Resident.FirstName} {c.Resident.LastName}",
        TenantDNI     = c.Resident.DNI,
        UnitNumber    = c.Unit.UnitNumber,
        BuildingName  = c.Unit.Building.Name,
        StartDate     = c.StartDate,
        EndDate       = c.EndDate,
        MonthlyRent   = c.MonthlyRent,
        DepositAmount = c.DepositAmount,
        CreditBalance = c.CreditBalance,
        Status        = c.Status.ToString(),
        PendingMonths = pending.Count,
        TotalDebt     = effectiveDebt
    };
}

    // GET: api/rental/filter
    [HttpGet("filter")]
    public async Task<ActionResult<ICollection<GetRentalOutput>>> FilterRentals(
        [FromQuery] string? status,
        [FromQuery] int? residentId)
    {
        var query = _context.RentalContracts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<RentalContractStatus>(status, ignoreCase: true, out var parsedStatus))
                query = query.Where(c => c.Status == parsedStatus);
            else
                return BadRequest($"Estado '{status}' no válido. Use: Active, Terminated, Cancelled.");
        }

        if (residentId.HasValue)
            query = query.Where(c => c.ResidentId == residentId.Value);

        var contracts = await query
            .Include(c => c.Resident)
            .Include(c => c.Unit)
                .ThenInclude(u => u.Building)
            .Include(c => c.Payments)
            .AsNoTracking()
            .ToListAsync();

        return Ok(contracts.Select(MapToOutput).ToList());
    }
}