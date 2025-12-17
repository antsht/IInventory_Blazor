# Active Context: Equipment Inventory System

## Current State
**Date**: December 17, 2025  
**Status**: Fully functional application with all core features implemented

## What's Working
- ✅ Equipment CRUD operations
- ✅ Inventory numbers (инвентарные номера) as barcodes
- ✅ Equipment search and filtering
- ✅ Barcode printing (opens print dialog)
- ✅ Inventory audit creation
- ✅ Barcode scanning (manual input)
- ✅ Audit statistics (total/found/not found)
- ✅ Audit completion
- ✅ CSV report generation and download
- ✅ Responsive UI design
- ✅ Russian language interface
- ✅ **Standalone справочник pages** (`/employees`, `/workplaces`)
- ✅ Employee management with sortable table, search, filter
- ✅ Workplace management with sortable table, search, filter
- ✅ Equipment splitting/duplication feature
- ✅ Sortable columns in all tables
- ✅ Mark equipment as found/not found in audit
- ✅ **CSV data import** (DataSeedService)
- ✅ Navigation dropdown menu for справочники

## Recent Changes (December 17, 2025)
- **Added**: Standalone справочник pages (`Employees.razor`, `Workplaces.razor`)
  - Sortable columns with visual sort indicators
  - Search functionality
  - Toggle to show/hide inactive records
  - Full CRUD with confirmation dialogs
- **Added**: Navigation dropdown menu for Справочники
- **Added**: `DataSeedService` for CSV bulk import
- **Fixed**: `Home.razor` now implements `IDisposable` interface (Timer disposal)
- **Fixed**: `BarcodeScannerModal.razor` - `DotNetObjectReference` memory leak fixed
- **Fixed**: Equipment split now properly loads navigation properties
- **Added**: Delete confirmation in справочниках with usage check
- Memory Bank updated with current project state

## Current Focus
- Application is feature-complete for core requirements
- Code quality and resource management improvements completed

## Next Steps
Potential improvements and features:
1. Camera-based barcode scanning (using device camera)
2. Bulk equipment import UI (CSV/Excel upload)
3. Equipment history tracking
4. User authentication and roles
5. Print multiple barcodes at once
6. Equipment categories/tags
7. Dashboard with analytics
8. Export equipment list to Excel
9. Dark mode
10. Offline support (PWA)

## Active Decisions
- Using SQLite for simplicity (no external DB server needed)
- Russian-only UI (no i18n framework yet)
- Inventory number = Barcode (SerialNumber field stores it)
- SVG-based barcode rendering (simple binary representation)
- Standalone справочник pages instead of modal-only management

## Known Considerations
- Blazor Server requires stable connection
- Popup blockers may interfere with barcode printing
- No offline capability
- No authentication (single-user assumed)
- DataSeedService reads from `data.csv` at startup if DB is empty

## Architecture Notes
- `IDbContextFactory` pattern used throughout for thread-safe DB operations
- Services registered as Scoped in Program.cs
- Modal components use EventCallback pattern for parent communication
- All справочник tables have full sorting support
- Navigation uses dropdown menu for справочники section
