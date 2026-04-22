using CondoAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CondoAdmin.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Building>           Buildings           => Set<Building>();
    public DbSet<Unit>               Units               => Set<Unit>();
    public DbSet<Resident>           Residents           => Set<Resident>();
    public DbSet<Payment>            Payments            => Set<Payment>();
    public DbSet<Visitor>            Visitors            => Set<Visitor>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<Sale>               Sales               => Set<Sale>(); 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Building
        modelBuilder.Entity<Building>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Address).IsRequired().HasMaxLength(200);
        });

        // Unit
        modelBuilder.Entity<Unit>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UnitNumber).IsRequired().HasMaxLength(10);
            e.Property(x => x.MonthlyFee).HasColumnType("decimal(10,2)");
            e.Property(x => x.AreaM2).HasColumnType("decimal(8,2)");
            e.HasOne(x => x.Building)
             .WithMany(b => b.Units)
             .HasForeignKey(x => x.BuildingId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Resident
        modelBuilder.Entity<Resident>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DNI).IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.DNI).IsUnique();
            e.HasOne(x => x.Unit)
             .WithMany(u => u.Residents)
             .HasForeignKey(x => x.UnitId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment
        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(10,2)");
            e.HasOne(x => x.Resident)
             .WithMany(r => r.Payments)
             .HasForeignKey(x => x.ResidentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Visitor
        modelBuilder.Entity<Visitor>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Unit)
             .WithMany(u => u.Visitors)
             .HasForeignKey(x => x.UnitId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // MaintenanceRequest
        modelBuilder.Entity<MaintenanceRequest>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Unit)
             .WithMany(u => u.MaintenanceRequests)
             .HasForeignKey(x => x.UnitId)
             .OnDelete(DeleteBehavior.Restrict);
        });

         // Sale
        modelBuilder.Entity<Sale>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.SalePrice).HasColumnType("decimal(12,2)");
            e.Property(x => x.MethodOfPayment).IsRequired().HasMaxLength(50);
            e.HasOne(x => x.Resident)
            .WithMany(r => r.Sales)
            .HasForeignKey(x => x.ResidentId)
            .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Unit)
            .WithMany(u => u.Sales) // 👈
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
        });
    }
}