# Technical Context: Equipment Inventory System

## Technology Stack

### Framework & Runtime
- **.NET 9.0** - Latest .NET runtime
- **Blazor Server** - Interactive server-side rendering
- **C#** - Primary programming language

### Database
- **SQLite** - Lightweight file-based database (`inventory.db`)
- **Entity Framework Core 9.0** - ORM for database operations
- **IDbContextFactory** - Thread-safe context creation for Blazor Server

### Frontend
- **Razor Components** - Blazor component model
- **Bootstrap** - CSS framework (via wwwroot/lib/bootstrap)
- **Custom CSS** - Modern styling with CSS variables (`app.css`)
- **Inter font** - Google Fonts for typography
- **SVG icons** - Inline SVG icons throughout

### JavaScript Interop
- `printBarcode()` - Open popup window for barcode printing
- `downloadFile()` - Trigger file download for CSV reports
- `addEscListener()` - ESC key handling for modals

## Development Setup

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 / VS Code / Cursor IDE

### Running the Application
```bash
cd D:\IInventory_Blazor
dotnet run
```

### Default Ports
- HTTP: 5003 (configured in launchSettings.json)
- Check `Properties/launchSettings.json` for exact ports

### Database
- Auto-created on first run via `EnsureCreatedAsync()`
- File location: `inventory.db` in project root
- No migrations needed - uses code-first approach

## Technical Constraints

### Blazor Server Specifics
- Server-side state management
- SignalR connection required
- Use `IDbContextFactory` instead of scoped `DbContext`
- `@rendermode InteractiveServer` required for interactivity
- Components must implement `IDisposable` when using timers or `DotNetObjectReference`

### Browser Requirements
- Modern browser with WebSocket support
- JavaScript enabled for barcode printing and file download
- Popup windows allowed for barcode printing

## Dependencies (NuGet)
```xml
Microsoft.EntityFrameworkCore.Design 9.0.0
Microsoft.EntityFrameworkCore.Sqlite 9.0.0
Microsoft.EntityFrameworkCore.Tools 9.0.0
```

## Project Structure
```
IInventory_Blazor/
├── Components/
│   ├── Pages/           # Routable page components
│   ├── Layout/          # Layout components
│   ├── App.razor        # Root component
│   └── Routes.razor     # Router configuration
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── Equipment.cs     # + EquipmentTypes, EquipmentStatuses
│   ├── Employee.cs
│   ├── Workplace.cs
│   ├── InventoryAudit.cs  # + AuditStatuses
│   └── AuditItem.cs
├── Services/
│   ├── EquipmentService.cs
│   ├── EmployeeService.cs
│   ├── WorkplaceService.cs
│   ├── AuditService.cs
│   └── BarcodeService.cs
├── wwwroot/
│   ├── app.css          # Main styles
│   └── js/app.js        # JS interop functions
├── Program.cs           # App configuration
└── inventory.db         # SQLite database
```

## Key Patterns

### Service Registration
```csharp
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddScoped<EquipmentService>();
builder.Services.AddScoped<AuditService>();
// etc.
```

### Component Lifecycle
```csharp
@implements IDisposable

protected override async Task OnInitializedAsync() { }
protected override async Task OnAfterRenderAsync(bool firstRender) { }
public void Dispose() { /* cleanup */ }
```
