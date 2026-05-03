using CondoAdmin.Application.DTO.Sale.GetSale;
using CondoAdmin.Application.DTO.Sale.TriggerSale;
using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
using CondoAdmin.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers;
public class SalesController : BaseApiController
{
    private readonly AppDbContext _context;

    public SalesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<GetSaleOutput>>> GetSales()
    {
        var sales = await _context.Sales
            .Include(s => s.Resident)
            .Include(s => s.Unit)
                .ThenInclude(u => u.Building)
            .AsNoTracking()
            .ToListAsync();

        return Ok(sales.Select(s => new GetSaleOutput
        {
            Id              = s.Id,
            SaleDate        = s.SaleDate,
            SalePrice       = s.SalePrice,
            MethodOfPayment = s.MethodOfPayment,
            Notes           = s.Notes,
            ResidentId      = s.ResidentId,
            BuyerName       = $"{s.Resident.FirstName} {s.Resident.LastName}",
            BuyerDNI        = s.Resident.DNI,
            UnitId          = s.UnitId,
            UnitNumber      = s.Unit.UnitNumber,
            BuildingName    = s.Unit.Building.Name
        }).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetSaleOutput>> GetSale(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.Resident)
            .Include(s => s.Unit)
                .ThenInclude(u => u.Building)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale is null)
            return NotFound($"No se encontró la venta con ID {id}.");

        return Ok(new GetSaleOutput
        {
            Id              = sale.Id,
            SaleDate        = sale.SaleDate,
            SalePrice       = sale.SalePrice,
            MethodOfPayment = sale.MethodOfPayment,
            Notes           = sale.Notes,
            ResidentId      = sale.ResidentId,
            BuyerName       = $"{sale.Resident.FirstName} {sale.Resident.LastName}",
            BuyerDNI        = sale.Resident.DNI,
            UnitId          = sale.UnitId,
            UnitNumber      = sale.Unit.UnitNumber,
            BuildingName    = sale.Unit.Building.Name
        });
    }

    [HttpGet("resident/{dni}")]
    public async Task<ActionResult<ICollection<GetSaleOutput>>> GetSalesByResident(string dni)
    {
        var sales = await _context.Sales
            .Include(s => s.Resident)
            .Include(s => s.Unit)
                .ThenInclude(u => u.Building)
            .Where(s => s.Resident.DNI == dni)
            .AsNoTracking()
            .ToListAsync();

        if (!sales.Any())
            return NotFound($"No se encontraron ventas para el DNI '{dni}'.");

        return Ok(sales.Select(s => new GetSaleOutput
        {
            Id              = s.Id,
            SaleDate        = s.SaleDate,
            SalePrice       = s.SalePrice,
            MethodOfPayment = s.MethodOfPayment,
            Notes           = s.Notes,
            ResidentId      = s.ResidentId,
            BuyerName       = $"{s.Resident.FirstName} {s.Resident.LastName}",
            BuyerDNI        = s.Resident.DNI,
            UnitId          = s.UnitId,
            UnitNumber      = s.Unit.UnitNumber,
            BuildingName    = s.Unit.Building.Name
        }).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<TriggerSaleOutput>> TriggerSale([FromBody] TriggerSaleInput input)
    {
        var transactionDate = DateTime.UtcNow;

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
                MoveInDate = transactionDate,
                IsActive   = true
            };
            _context.Residents.Add(resident);
            await _context.SaveChangesAsync();
        }

        var outputDetails = new List<SaleDetailOutput>();
        decimal grandTotal = 0;

        foreach (var item in input.Details)
        {
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.UnitNumber == item.UnitNumber.Trim());

            if (unit is null)
                return BadRequest($"La unidad '{item.UnitNumber}' no existe.");
            if (unit.Status != UnitStatus.Available)
    return BadRequest($"La unidad '{item.UnitNumber}' no está disponible (estado: {unit.Status}).");


            var sale = new Sale
            {
                SalePrice       = item.Price,
                MethodOfPayment = input.MethodOfPayment,
                Notes           = item.Notes,
                SaleDate        = transactionDate,
                ResidentId      = resident.Id,
                UnitId          = unit.Id
            };

            _context.Sales.Add(sale);
            unit.Status = UnitStatus.Sold;
            grandTotal += sale.SalePrice;

            outputDetails.Add(new SaleDetailOutput
            {
                UnitNumber   = unit.UnitNumber,
                Floor        = unit.Floor,
                SalePrice    = sale.SalePrice,
                Notes        = item.Notes,
                NameBuilding = item.NameBuilding
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new TriggerSaleOutput
        {
            Buyer           = $"{resident.FirstName} {resident.LastName}",
            DNI             = resident.DNI,
            MethodOfPayment = input.MethodOfPayment,
            SaleDate        = transactionDate,
            Total           = grandTotal,
            Units           = outputDetails
        });
    }
}