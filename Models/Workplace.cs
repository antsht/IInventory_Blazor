using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models;

public class Workplace
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Building { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Floor { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Room { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    // Foreign key to Employee
    public Guid? EmployeeId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Employee? Employee { get; set; }
    public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
}

