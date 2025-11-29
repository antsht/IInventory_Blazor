using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;
using System.Text;

namespace InventoryApp.Services;

public class AuditService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

    public async Task<List<InventoryAudit>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.InventoryAudits
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<InventoryAudit?> GetByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.InventoryAudits
            .Include(a => a.AuditItems)
            .ThenInclude(ai => ai.Equipment)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<InventoryAudit> CreateAsync(string auditor)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var audit = new InventoryAudit
        {
            Id = Guid.NewGuid(),
            Auditor = auditor,
            Status = "in_progress",
            AuditDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        context.InventoryAudits.Add(audit);
        await context.SaveChangesAsync();

        return audit;
    }

    public async Task<bool> CompleteAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var audit = await context.InventoryAudits.FindAsync(id);
        if (audit == null) return false;

        audit.Status = "completed";
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<AuditItem>> GetScannedItemsAsync(Guid auditId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AuditItems
            .Include(ai => ai.Equipment)
            .Where(ai => ai.AuditId == auditId)
            .OrderByDescending(ai => ai.ScannedAt)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> ScanBarcodeAsync(Guid auditId, string barcode)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var equipment = await context.Equipment.FirstOrDefaultAsync(e => e.Barcode == barcode);
        if (equipment == null)
        {
            return (false, $"Оборудование с штрихкодом {barcode} не найдено");
        }

        var alreadyScanned = await context.AuditItems
            .AnyAsync(ai => ai.AuditId == auditId && ai.EquipmentId == equipment.Id);
        
        if (alreadyScanned)
        {
            return (false, "Это оборудование уже отсканировано в этой инвентаризации");
        }

        var auditItem = new AuditItem
        {
            Id = Guid.NewGuid(),
            AuditId = auditId,
            EquipmentId = equipment.Id,
            Found = true,
            ScannedAt = DateTime.UtcNow
        };

        context.AuditItems.Add(auditItem);
        await context.SaveChangesAsync();

        return (true, $"Оборудование \"{equipment.Name}\" успешно отсканировано");
    }

    public async Task<(int Total, int Found, int NotFound)> GetAuditStatsAsync(Guid auditId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var totalEquipment = await context.Equipment.CountAsync();
        var foundItems = await context.AuditItems
            .Where(ai => ai.AuditId == auditId)
            .CountAsync();

        return (totalEquipment, foundItems, totalEquipment - foundItems);
    }

    public async Task<List<Equipment>> GetNotFoundEquipmentAsync(Guid auditId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var scannedIds = await context.AuditItems
            .Where(ai => ai.AuditId == auditId)
            .Select(ai => ai.EquipmentId)
            .ToListAsync();

        return await context.Equipment
            .Where(e => !scannedIds.Contains(e.Id))
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> UnmarkAsFoundAsync(Guid auditId, Guid equipmentId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var auditItem = await context.AuditItems
            .FirstOrDefaultAsync(ai => ai.AuditId == auditId && ai.EquipmentId == equipmentId);
        
        if (auditItem == null)
        {
            return (false, "Запись не найдена");
        }

        context.AuditItems.Remove(auditItem);
        await context.SaveChangesAsync();

        return (true, "Оборудование перенесено в ненайденное");
    }

    public async Task<string> GenerateCsvReportAsync(Guid auditId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var audit = await context.InventoryAudits.FindAsync(auditId);
        if (audit == null) return string.Empty;

        var scannedItems = await GetScannedItemsAsync(auditId);
        var notFoundEquipment = await GetNotFoundEquipmentAsync(auditId);

        var csv = new StringBuilder();
        csv.AppendLine("Статус,Название,Тип,Модель,Штрихкод,Местоположение,Назначено");

        foreach (var item in scannedItems)
        {
            csv.AppendLine($"Найдено,\"{item.Equipment.Name}\",\"{item.Equipment.Type}\",\"{item.Equipment.Model}\",\"{item.Equipment.Barcode}\",\"{item.Equipment.Location}\",\"{item.Equipment.AssignedTo}\"");
        }

        foreach (var eq in notFoundEquipment)
        {
            csv.AppendLine($"Не найдено,\"{eq.Name}\",\"{eq.Type}\",\"{eq.Model}\",\"{eq.Barcode}\",\"{eq.Location}\",\"{eq.AssignedTo}\"");
        }

        return csv.ToString();
    }
}


