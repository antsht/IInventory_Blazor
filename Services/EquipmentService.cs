using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;

namespace InventoryApp.Services;

public class EquipmentService(IDbContextFactory<ApplicationDbContext> contextFactory, BarcodeService barcodeService)
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;
    private readonly BarcodeService _barcodeService = barcodeService;

    public async Task<List<Equipment>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Equipment
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<Equipment?> GetByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Equipment.FindAsync(id);
    }

    public async Task<Equipment?> GetByBarcodeAsync(string barcode)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Equipment.FirstOrDefaultAsync(e => e.Barcode == barcode);
    }

    public async Task<Equipment> CreateAsync(Equipment equipment)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        equipment.Id = Guid.NewGuid();
        equipment.Barcode = _barcodeService.GenerateBarcode();
        equipment.CreatedAt = DateTime.UtcNow;
        equipment.UpdatedAt = DateTime.UtcNow;

        context.Equipment.Add(equipment);
        await context.SaveChangesAsync();

        return equipment;
    }

    public async Task<Equipment?> UpdateAsync(Equipment equipment)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Equipment.FindAsync(equipment.Id);
        if (existing == null) return null;

        existing.Name = equipment.Name;
        existing.Type = equipment.Type;
        existing.Manufacturer = equipment.Manufacturer;
        existing.Model = equipment.Model;
        existing.SerialNumber = equipment.SerialNumber;
        existing.PurchaseDate = equipment.PurchaseDate;
        existing.Status = equipment.Status;
        existing.Location = equipment.Location;
        existing.AssignedTo = equipment.AssignedTo;
        existing.Notes = equipment.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var equipment = await context.Equipment.FindAsync(id);
        if (equipment == null) return false;

        context.Equipment.Remove(equipment);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Equipment>> SearchAsync(string? searchTerm, string? typeFilter)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Equipment.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e =>
                e.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                e.Barcode.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                e.Model.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                e.SerialNumber.Contains(term, StringComparison.CurrentCultureIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(typeFilter) && typeFilter != "all")
        {
            query = query.Where(e => e.Type == typeFilter);
        }

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<Equipment> DuplicateAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var original = await context.Equipment.FindAsync(id) ?? throw new InvalidOperationException("Оборудование не найдено");
        var duplicate = new Equipment
        {
            Id = Guid.NewGuid(),
            Barcode = original.Barcode, // Сохраняем тот же инвентарный номер
            Name = original.Name,
            Type = original.Type,
            Manufacturer = original.Manufacturer,
            Model = original.Model,
            SerialNumber = original.SerialNumber,
            PurchaseDate = original.PurchaseDate,
            Status = original.Status,
            Location = original.Location,
            AssignedTo = original.AssignedTo,
            Notes = string.IsNullOrEmpty(original.Notes) 
                ? "(Разделено)" 
                : $"{original.Notes} (Разделено)",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Equipment.Add(duplicate);
        await context.SaveChangesAsync();

        return duplicate;
    }
}


