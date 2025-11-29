using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models;

public class InventoryAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime AuditDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(200)]
    public string Auditor { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "in_progress";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AuditItem> AuditItems { get; set; } = new List<AuditItem>();
}

public static class AuditStatuses
{
    public static readonly Dictionary<string, string> All = new()
    {
        { "in_progress", "В процессе" },
        { "completed", "Завершено" }
    };
}


