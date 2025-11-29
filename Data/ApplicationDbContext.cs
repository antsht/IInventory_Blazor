using Microsoft.EntityFrameworkCore;
using InventoryApp.Models;

namespace InventoryApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Equipment> Equipment { get; set; } = null!;
    public DbSet<InventoryAudit> InventoryAudits { get; set; } = null!;
    public DbSet<AuditItem> AuditItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Equipment configuration
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Barcode); // Не уникальный - один инв. номер может быть у нескольких частей оборудования
            entity.Property(e => e.Barcode).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Type).IsRequired().HasDefaultValue("PC");
            entity.Property(e => e.Status).IsRequired().HasDefaultValue("active");
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


