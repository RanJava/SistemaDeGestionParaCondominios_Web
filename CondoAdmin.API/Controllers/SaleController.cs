using CondoAdmin.Application.DTO.Sale.TriggerSale;
using CondoAdmin.Domain.Entities;
using CondoAdmin.Infrastructure.Persistence;
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
                LastName = input.LastName!,
                DNI = input.DNI,
                Email = input.Email!,
                Phone = input.Phone!,
            };

            resident.MoveInDate = transactionDate;
            resident.IsActive = true;

            _context.Residents.Add(resident);
            await _context.SaveChangesAsync();
        }

        var outputDetails = new List<SaleDetailOutput>();
        var grandTotal = 0;

        foreach (var item in input.Details)
        {
            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.UnitNumber == item.UnitNumber.Trim());

            if (unit is null)
                return BadRequest($"La unidad '{item.UnitNumber}' no existe.");
            if (unit.Status == true)
                return BadRequest($"La unidad '{item.UnitNumber}' ya fue vendida.");

            var sale = new Sale
            {
                SalePrice = item.Price,
                MethodOfPayment = input.MethodOfPayment,
                Notes = item.Notes,
            };
            sale.SaleDate = transactionDate;
            sale.ResidentId = resident.Id;
            sale.UnitId = unit.Id;

            _context.Sales.Add(sale);
            unit.Status = true;

            grandTotal += sale.SalePrice;

            outputDetails.Add(new SaleDetailOutput
            {
                UnitNumber = unit.UnitNumber,
                Floor = unit.Floor,
                SalePrice = sale.SalePrice,
                Notes = item.Notes,
                NameBuilding = item.NameBuilding
            });
        }
        await _context.SaveChangesAsync();


        var output = new TriggerSaleOutput
        {
            Buyer = $"{resident.FirstName} {resident.LastName}",
            DNI = resident.DNI,
            MethodOfPayment = input.MethodOfPayment,
            SaleDate = transactionDate,
            Total = grandTotal,
            Units = outputDetails
        };

        return Ok(output);
    }
}