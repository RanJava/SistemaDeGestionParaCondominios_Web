using CondoAdmin.Application.DTO.Dashboard;
using CondoAdmin.Domain.Enums;
using CondoAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.API.Controllers
{
    public class DashboardController : BaseApiController
    {
        private readonly AppDbContext _contexto;

        public DashboardController(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        // GET: api/dashboard
        [HttpGet]
        public async Task<ActionResult<DashboardOutput>> GetDashboard()
        {
            var today = DateTime.Today;

            var buildingTotal  = await _contexto.Buildings.CountAsync();
            var buildingActive = await _contexto.Buildings.CountAsync(b => b.IsActive);

            var unitTotal     = await _contexto.Units.CountAsync();
            var unitAvailable = await _contexto.Units.CountAsync(u => u.Status == UnitStatus.Available);
            var unitSold      = await _contexto.Units.CountAsync(u => u.Status == UnitStatus.Sold);
            var unitRented    = await _contexto.Units.CountAsync(u => u.Status == UnitStatus.Rented);

            var residentTotal  = await _contexto.Residents.CountAsync();
            var residentActive = await _contexto.Residents.CountAsync(r => r.IsActive);

            var pendingPayments = await _contexto.Payments
                .Where(p => p.PaidAt == null && p.DueDate < today)
                .ToListAsync();

            var maintenancePending  = await _contexto.MaintenanceRequests.CountAsync(m => m.ResolvedAt == null);
            var maintenanceResolved = await _contexto.MaintenanceRequests.CountAsync(m => m.ResolvedAt != null);

            var output = new DashboardOutput
            {
                Buildings = new BuildingSummary
                {
                    Total  = buildingTotal,
                    Active = buildingActive
                },
                Units = new UnitSummary
                {
                    Total     = unitTotal,
                    Available = unitAvailable,
                    Sold      = unitSold,
                    Rented    = unitRented
                },
                Residents = new ResidentSummary
                {
                    Total  = residentTotal,
                    Active = residentActive
                },
                Payments = new PaymentSummary
                {
                    PendingCount = pendingPayments.Count,
                    TotalDebt    = pendingPayments.Sum(p => p.Amount)
                },
                Maintenance = new MaintenanceSummary
                {
                    PendingCount  = maintenancePending,
                    ResolvedCount = maintenanceResolved
                }
            };

            return Ok(output);
        }
    }
}