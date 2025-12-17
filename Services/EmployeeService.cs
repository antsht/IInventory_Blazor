using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;

namespace InventoryApp.Services;

public class EmployeeService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

    public async Task<List<Employee>> GetAllAsync(bool includeInactive = false)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Employees.AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }
        
        return await query
            .OrderBy(e => e.FullName)
            .ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Employees.FindAsync(id);
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        employee.Id = Guid.NewGuid();
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;

        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        return employee;
    }

    public async Task<Employee?> UpdateAsync(Employee employee)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Employees.FindAsync(employee.Id);
        if (existing == null) return null;

        existing.FullName = employee.FullName;
        existing.Position = employee.Position;
        existing.Department = employee.Department;
        existing.Email = employee.Email;
        existing.Phone = employee.Phone;
        existing.IsActive = employee.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var employee = await context.Employees.FindAsync(id);
        if (employee == null) return false;

        // Soft delete - just mark as inactive
        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HardDeleteAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var employee = await context.Employees.FindAsync(id);
        if (employee == null) return false;

        context.Employees.Remove(employee);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Employee>> SearchAsync(string? searchTerm)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Employees.Where(e => e.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e =>
                e.FullName.ToLower().Contains(term) ||
                e.Position.ToLower().Contains(term) ||
                e.Department.ToLower().Contains(term));
        }

        return await query
            .OrderBy(e => e.FullName)
            .ToListAsync();
    }

    /// <summary>
    /// Проверяет, используется ли сотрудник в оборудовании или рабочих местах
    /// </summary>
    public async Task<(bool IsUsed, int EquipmentCount, int WorkplaceCount)> CheckUsageAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var equipmentCount = await context.Equipment.CountAsync(e => e.EmployeeId == id);
        var workplaceCount = await context.Workplaces.CountAsync(w => w.EmployeeId == id && w.IsActive);
        
        return (equipmentCount > 0 || workplaceCount > 0, equipmentCount, workplaceCount);
    }
}

