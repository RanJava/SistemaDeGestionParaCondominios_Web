using CondoAdmin.Application.DTO.Report;
using CondoAdmin.Application.DTO.Report.DebtorReport;
using CondoAdmin.Application.DTO.Report.IncomeReport;
using CondoAdmin.Application.DTO.Report.OccupancyReport;
using CondoAdmin.Domain.Enums;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers
{
    public class ReportController : BaseApiController
    {
        private readonly AppDbContext _contexto;

        public ReportController(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        // GET: api/report/debtors
        [HttpGet("debtors")]
        public async Task<ActionResult<ICollection<DebtorReportOutput>>> GetDebtorReport()
        {
            var today = DateTime.Today;

            var buildings = await _contexto.Buildings
                .AsNoTracking()
                .ToListAsync();

            var result = new List<DebtorReportOutput>();

            foreach (var building in buildings)
            {
                var debtors = await _contexto.Residents
                    .AsNoTracking()
                    .Where(r => r.Unit != null &&
                                r.Unit.BuildingId == building.Id &&
                                r.Payments.Any(p => p.PaidAt == null && p.DueDate < today))
                    .Select(r => new DebtorDetail
                    {
                        ResidentName = $"{r.FirstName} {r.LastName}",
                        DNI = r.DNI,
                        UnitNumber = r.Unit != null ? r.Unit.UnitNumber : "Sin unidad",
                        PendingPayments = r.Payments.Count(p => p.PaidAt == null && p.DueDate < today),
                        TotalDebt = r.Payments
                            .Where(p => p.PaidAt == null && p.DueDate < today)
                            .Sum(p => p.Amount)
                    })
                    .OrderByDescending(r => r.TotalDebt)
                    .ToListAsync();

                result.Add(new DebtorReportOutput
                {
                    BuildingName  = building.Name,
                    TotalDebtors  = debtors.Count,
                    TotalDebt     = debtors.Sum(d => d.TotalDebt),
                    Debtors       = debtors
                });
            }

            return Ok(result);
        }

        // GET: api/report/income/{buildingId}
        [HttpGet("income/{buildingId}")]
        public async Task<ActionResult<IncomeReportOutput>> GetIncomeReport(int buildingId)
        {
            var building = await _contexto.Buildings.FindAsync(buildingId);
            if (building == null)
                return NotFound($"No se encontró el edificio con ID {buildingId}.");

            var payments = await _contexto.Payments
                .AsNoTracking()
                .Where(p => p.Resident.Unit != null &&
                            p.Resident.Unit.BuildingId == buildingId)
                .ToListAsync();

            var details = payments
                .GroupBy(p => p.Month)
                .Select(g => new IncomeDetail
                {
                    Month     = g.Key,
                    Collected = g.Where(p => p.PaidAt != null).Sum(p => p.Amount),
                    Pending   = g.Where(p => p.PaidAt == null).Sum(p => p.Amount)
                })
                .ToList();

            return Ok(new IncomeReportOutput
            {
                BuildingName   = building.Name,
                TotalCollected = details.Sum(d => d.Collected),
                TotalPending   = details.Sum(d => d.Pending),
                Details        = details
            });
        }

        // GET: api/report/occupancy
        [HttpGet("occupancy")]
        public async Task<ActionResult<ICollection<OccupancyReportOutput>>> GetOccupancyReport()
        {
            var buildings = await _contexto.Buildings
                .AsNoTracking()
                .ToListAsync();

            var result = new List<OccupancyReportOutput>();

            foreach (var building in buildings)
            {
                var units = await _contexto.Units
                    .AsNoTracking()
                    .Where(u => u.BuildingId == building.Id)
                    .ToListAsync();

                var total     = units.Count;
                var available = units.Count(u => u.Status == UnitStatus.Available);
                var sold      = units.Count(u => u.Status == UnitStatus.Sold);
                var rented    = units.Count(u => u.Status == UnitStatus.Rented);
                var rate      = total > 0
                                ? Math.Round((decimal)(sold + rented) / total * 100, 2)
                                : 0;

                result.Add(new OccupancyReportOutput
                {
                    BuildingName  = building.Name,
                    TotalUnits    = total,
                    Available     = available,
                    Sold          = sold,
                    Rented        = rented,
                    OccupancyRate = rate
                });
            }

            return Ok(result);
        }
    }
}