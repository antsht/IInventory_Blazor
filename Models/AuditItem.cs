using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models;

public class AuditItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid AuditId { get; set; }

    public InventoryAudit Audit { get; set; } = null!;

    [Required]
    public Guid EquipmentId { get; set; }

    public Equipment Equipment { get; set; } = null!;

    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    public bool Found { get; set; } = true;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}


