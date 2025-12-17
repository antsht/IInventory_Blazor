using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models;

public class Equipment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string Barcode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = "PC";

    [MaxLength(100)]
    public string Manufacturer { get; set; } = string.Empty;

    [MaxLength(100)]
    public string SerialNumber { get; set; } = string.Empty;

    public DateOnly? PurchaseDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "active";

    // Foreign key to Workplace
    public Guid? WorkplaceId { get; set; }

    // Foreign key to Employee (can be different from Workplace.EmployeeId)
    public Guid? EmployeeId { get; set; }

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Workplace? Workplace { get; set; }
    public Employee? Employee { get; set; }
    public ICollection<AuditItem> AuditItems { get; set; } = [];
}

public static class EquipmentTypes
{
    public static readonly Dictionary<string, string> All = new()
    {
        { "PC", "Компьютер" },
        { "Laptop", "Ноутбук" },
        { "Monitor", "Монитор" },
        { "Printer", "Принтер" },
        { "Scanner", "Сканер" },
        { "MFP", "МФУ" },
        { "Server", "Сервер" },
        { "Network", "Сетевое оборудование" },
        { "Other", "Другое" }
    };
}

public static class EquipmentStatuses
{
    public static readonly Dictionary<string, string> All = new()
    {
        { "active", "Активно" },
        { "inactive", "Неактивно" },
        { "repair", "Ремонт" },
        { "disposed", "Списано" }
    };
}
