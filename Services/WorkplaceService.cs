using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;

namespace InventoryApp.Services;

public class WorkplaceService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

    public async Task<List<Workplace>> GetAllAsync(bool includeInactive = false)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Workplaces
            .Include(w => w.Employee)
            .AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(w => w.IsActive);
        }
        
        return await query
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<Workplace?> GetByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Workplaces
            .Include(w => w.Employee)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Workplace> CreateAsync(Workplace workplace)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        workplace.Id = Guid.NewGuid();
        workplace.CreatedAt = DateTime.UtcNow;
        workplace.UpdatedAt = DateTime.UtcNow;

        context.Workplaces.Add(workplace);
        await context.SaveChangesAsync();

        // Reload with Employee navigation property
        return await GetByIdAsync(workplace.Id) ?? workplace;
    }

    public async Task<Workplace?> UpdateAsync(Workplace workplace)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Workplaces.FindAsync(workplace.Id);
        if (existing == null) return null;

        existing.Name = workplace.Name;
        existing.Building = workplace.Building;
        existing.Floor = workplace.Floor;
        existing.Room = workplace.Room;
        existing.Description = workplace.Description;
        existing.EmployeeId = workplace.EmployeeId;
        existing.IsActive = workplace.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        
        // Reload with Employee navigation property
        return await GetByIdAsync(existing.Id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var workplace = await context.Workplaces.FindAsync(id);
        if (workplace == null) return false;

        // Soft delete - just mark as inactive
        workplace.IsActive = false;
        workplace.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HardDeleteAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var workplace = await context.Workplaces.FindAsync(id);
        if (workplace == null) return false;

        context.Workplaces.Remove(workplace);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Workplace>> SearchAsync(string? searchTerm)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Workplaces
            .Include(w => w.Employee)
            .Where(w => w.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(w =>
                w.Name.ToLower().Contains(term) ||
                w.Building.ToLower().Contains(term) ||
                w.Room.ToLower().Contains(term));
        }

        return await query
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<List<Workplace>> GetByEmployeeIdAsync(Guid employeeId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Workplaces
            .Include(w => w.Employee)
            .Where(w => w.IsActive && w.EmployeeId == employeeId)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Проверяет, используется ли рабочее место в оборудовании
    /// </summary>
    public async Task<(bool IsUsed, int EquipmentCount)> CheckUsageAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var equipmentCount = await context.Equipment.CountAsync(e => e.WorkplaceId == id);
        
        return (equipmentCount > 0, equipmentCount);
    }
}

