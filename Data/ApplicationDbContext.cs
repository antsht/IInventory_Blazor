using Microsoft.EntityFrameworkCore;
using InventoryApp.Models;

namespace InventoryApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Equipment> Equipment { get; set; } = null!;
    public DbSet<InventoryAudit> InventoryAudits { get; set; } = null!;
    public DbSet<AuditItem> AuditItems { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Workplace> Workplaces { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired();
            entity.HasIndex(e => e.FullName);
        });

        // Workplace configuration
        modelBuilder.Entity<Workplace>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.HasIndex(e => e.Name);

            entity.HasOne(w => w.Employee)
                .WithMany(e => e.Workplaces)
                .HasForeignKey(w => w.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Equipment configuration
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Barcode);
            entity.Property(e => e.Barcode).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Type).IsRequired().HasDefaultValue("PC");
            entity.Property(e => e.Status).IsRequired().HasDefaultValue("active");

            entity.HasOne(eq => eq.Workplace)
                .WithMany(w => w.Equipment)
                .HasForeignKey(eq => eq.WorkplaceId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(eq => eq.Employee)
                .WithMany(e => e.Equipment)
                .HasForeignKey(eq => eq.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // InventoryAudit configuration
        modelBuilder.Entity<InventoryAudit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Auditor).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasDefaultValue("in_progress");
        });

        // AuditItem configuration
        modelBuilder.Entity<AuditItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Audit)
                .WithMany(a => a.AuditItems)
                .HasForeignKey(e => e.AuditId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Equipment)
                .WithMany(eq => eq.AuditItems)
                .HasForeignKey(e => e.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.AuditId);
            entity.HasIndex(e => e.EquipmentId);
        });
    }
}
