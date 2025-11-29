# System Patterns: Equipment Inventory System

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                      Blazor Server App                       │
├─────────────────────────────────────────────────────────────┤
│  Components/Pages/                                           │
│  ├── Home.razor          (Equipment catalog)                │
│  ├── Audit.razor         (Inventory audit)                  │
│  ├── EquipmentFormModal  (Add/Edit equipment)               │
│  └── BarcodeScannerModal (Barcode scanning)                 │
├─────────────────────────────────────────────────────────────┤
│  Services/                                                   │
│  ├── EquipmentService    (Equipment CRUD)                   │
│  ├── AuditService        (Audit operations)                 │
│  └── BarcodeService      (Barcode generation)               │
├─────────────────────────────────────────────────────────────┤
│  Data/                                                       │
│  └── ApplicationDbContext (EF Core DbContext)               │
├─────────────────────────────────────────────────────────────┤
│  Models/                                                     │
│  ├── Equipment           (Equipment entity)                 │
│  ├── InventoryAudit      (Audit entity)                     │
│  └── AuditItem           (Audit-Equipment junction)         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │   SQLite DB     │
                    │  inventory.db   │
                    └─────────────────┘
```

## Data Model

### Entities and Relationships
```
Equipment ←──────┐
    │            │ Many-to-Many via AuditItem
    │            │
InventoryAudit ──┘
```

### Equipment Model
- `Id` (Guid) - Primary key
- `Barcode` (string) - Unique, auto-generated
- `Name`, `Type`, `Manufacturer`, `Model`, `SerialNumber`
- `PurchaseDate` (DateOnly?), `Status`, `Location`, `AssignedTo`, `Notes`
- `CreatedAt`, `UpdatedAt` (DateTime)

### InventoryAudit Model
- `Id` (Guid) - Primary key
- `AuditDate`, `Auditor`, `Status`
- `CreatedAt` (DateTime)

### AuditItem Model (Junction)
- `Id` (Guid) - Primary key
- `AuditId`, `EquipmentId` - Foreign keys
- `ScannedAt` (DateTime), `Found` (bool), `Notes`

## Key Design Patterns

### Service Layer Pattern
Services encapsulate business logic and database operations:
```csharp
public class EquipmentService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    
    public async Task<List<Equipment>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Equipment.ToListAsync();
    }
}
```

### DbContextFactory Pattern (Blazor Server)
Using `IDbContextFactory` instead of scoped `DbContext` to avoid threading issues:
```csharp
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
```

### Component Parameter Pattern
Parent-child communication via parameters and callbacks:
```razor
<EquipmentFormModal Equipment="editingEquipment" 
                    OnClose="CloseForm" 
                    OnSaved="OnEquipmentSaved" />
```

### Static Lookup Pattern
Type-safe enumerations with display names:
```csharp
public static class EquipmentTypes
{
    public static readonly Dictionary<string, string> All = new()
    {
        { "PC", "Компьютер" },
        { "Laptop", "Ноутбук" },
        // ...
    };
}
```

## Component Responsibilities

| Component | Responsibility |
|-----------|---------------|
| `Home.razor` | Equipment list, search, filter, CRUD trigger |
| `Audit.razor` | Audit list, audit workflow, statistics |
| `EquipmentFormModal.razor` | Equipment form (add/edit) |
| `BarcodeScannerModal.razor` | Barcode input for scanning |
| `MainLayout.razor` | Navigation, header, layout structure |

## CSS Architecture
- CSS Variables for theming (colors, radii, shadows)
- Component-scoped styles where needed
- Utility classes for spacing (`.mb-6`)
- Status badges with semantic colors

