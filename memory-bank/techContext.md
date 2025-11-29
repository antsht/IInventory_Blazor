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
cd D:\!PROGRAMMING\IInventory_Blazor
dotnet run
```

### Default Ports
- HTTP: 5000 (or 5xxx)
- HTTPS: 5001 (or 7xxx)
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

