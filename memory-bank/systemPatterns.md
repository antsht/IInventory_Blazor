# System Patterns: Equipment Inventory System

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                      Blazor Server App                       │
├─────────────────────────────────────────────────────────────┤
│  Components/Pages/                                           │
│  ├── Home.razor              (Equipment catalog)            │
│  ├── Audit.razor             (Inventory audit)              │
│  ├── EquipmentFormModal      (Add/Edit equipment)           │
│  ├── BarcodeScannerModal     (Barcode scanning)             │
│  ├── ConfirmModal            (Confirmation dialogs)         │
│  ├── EmployeeEditorModal     (Employee справочник)          │
│  └── WorkplaceEditorModal    (Workplace справочник)         │
├─────────────────────────────────────────────────────────────┤
│  Services/                                                   │
│  ├── EquipmentService    (Equipment CRUD + search)          │
│  ├── AuditService        (Audit operations)                 │
│  ├── BarcodeService      (Barcode HTML generation)          │
│  ├── EmployeeService     (Employee CRUD)                    │
│  └── WorkplaceService    (Workplace CRUD)                   │
├─────────────────────────────────────────────────────────────┤
│  Data/                                                       │
│  └── ApplicationDbContext (EF Core DbContext)               │
├─────────────────────────────────────────────────────────────┤
│  Models/                                                     │
│  ├── Equipment           (Equipment entity)                 │
│  ├── InventoryAudit      (Audit entity)                     │
│  ├── AuditItem           (Audit-Equipment junction)         │
│  ├── Employee            (Employee entity)                  │
│  └── Workplace           (Workplace entity)                 │
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
Employee ──────────┐
    │ 1:N          │ 1:N (optional)
    ▼              │
Workplace ─────────┼──────── Equipment
    │ 1:N          │ 1:N        │
    └──────────────┘            │ Many-to-Many
                                │ via AuditItem
                                ▼
                         InventoryAudit
```

### Equipment Model
- `Id` (Guid) - Primary key
- `Barcode` (string) - Unique, equals to InventoryNumber
- `Name`, `Type`, `Manufacturer`, `Model`, `SerialNumber` (deprecated)
- `PurchaseDate` (DateOnly?), `Status`
- `WorkplaceId` (Guid?) - FK to Workplace
- `EmployeeId` (Guid?) - FK to Employee
- `Notes`, `CreatedAt`, `UpdatedAt`

### Employee Model
- `Id` (Guid) - Primary key
- `FullName`, `Position`, `Department`, `Email`, `Phone`
- `IsActive` (bool) - Soft delete flag
- Navigation: `Workplaces`, `Equipment`

### Workplace Model
- `Id` (Guid) - Primary key
- `Name`, `Building`, `Floor`, `Room`, `Description`
- `EmployeeId` (Guid?) - FK to responsible Employee
- `IsActive` (bool) - Soft delete flag
- Navigation: `Employee`, `Equipment`

### InventoryAudit Model
- `Id` (Guid) - Primary key
- `AuditDate`, `Auditor`, `Status`
- `CreatedAt` (DateTime)
- Navigation: `AuditItems`

### AuditItem Model (Junction)
- `Id` (Guid) - Primary key
- `AuditId`, `EquipmentId` - Foreign keys
- `ScannedAt` (DateTime), `Found` (bool), `Notes`

## Key Design Patterns

### Service Layer Pattern
Services encapsulate business logic and database operations:
```csharp
public class EquipmentService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;
    
    public async Task<List<Equipment>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Equipment
            .Include(e => e.Workplace)
            .Include(e => e.Employee)
            .ToListAsync();
    }
}
```

### DbContextFactory Pattern (Blazor Server)
Using `IDbContextFactory` instead of scoped `DbContext` to avoid threading issues:
```csharp
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
```

### IDisposable Pattern for Components
Components that use disposable resources must implement IDisposable:
```razor
@implements IDisposable

@code {
    private Timer? timer;
    private DotNetObjectReference<MyComponent>? dotNetRef;
    
    public void Dispose()
    {
        timer?.Dispose();
        dotNetRef?.Dispose();
    }
}
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

### Soft Delete Pattern
Employees and Workplaces use IsActive flag instead of hard delete:
```csharp
public async Task<bool> DeleteAsync(Guid id)
{
    var entity = await context.Find(id);
    entity.IsActive = false;
    await context.SaveChangesAsync();
}
```

## Component Responsibilities

| Component | Responsibility |
|-----------|---------------|
| `Home.razor` | Equipment list, search, filter, sort, CRUD trigger |
| `Audit.razor` | Audit list, audit workflow, statistics |
| `EquipmentFormModal.razor` | Equipment form (add/edit) |
| `BarcodeScannerModal.razor` | Barcode input for scanning |
| `ConfirmModal.razor` | Reusable confirmation dialog |
| `EmployeeEditorModal.razor` | Employee CRUD справочник |
| `WorkplaceEditorModal.razor` | Workplace CRUD справочник |
| `MainLayout.razor` | Navigation, header, layout structure |

## CSS Architecture
- CSS Variables for theming (colors, radii, shadows)
- Component-scoped styles where needed
- Utility classes for spacing (`.mb-6`)
- Status badges with semantic colors (`.status-{status}`)
