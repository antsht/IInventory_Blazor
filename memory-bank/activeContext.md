# Active Context: Equipment Inventory System

## Current State
**Date**: December 17, 2025  
**Status**: Functional application with core features implemented, bugs fixed

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
- ✅ Employee management (справочник сотрудников)
- ✅ Workplace management (справочник рабочих мест)
- ✅ Equipment splitting/duplication feature
- ✅ Sortable columns in equipment table
- ✅ Mark equipment as found/not found in audit

## Recent Changes (December 17, 2025)
- **Fixed**: `Home.razor` now implements `IDisposable` interface (Timer was not being disposed)
- **Fixed**: `BarcodeScannerModal.razor` - `DotNetObjectReference` memory leak fixed
- **Fixed**: Equipment split now properly loads navigation properties before editing
- Memory Bank updated with current project state

## Current Focus
- Code quality improvements
- Memory leak fixes
- Proper resource disposal

## Next Steps
Potential improvements and features:
1. Camera-based barcode scanning (using device camera)
2. Bulk equipment import (CSV/Excel)
3. Equipment history tracking
4. User authentication and roles
5. Print multiple barcodes at once
6. Equipment categories/tags
7. Dashboard with analytics
8. Export equipment list to Excel
9. Confirmation dialog before delete in справочниках (Workplaces/Employees)

## Active Decisions
- Using SQLite for simplicity (no external DB server needed)
- Russian-only UI (no i18n framework yet)
- Inventory number = Barcode (SerialNumber field stores it)
- SVG-based barcode rendering (simple binary representation)

## Known Considerations
- Blazor Server requires stable connection
- Popup blockers may interfere with barcode printing
- No offline capability
- No authentication (single-user assumed)

## Architecture Notes
- `IDbContextFactory` pattern used throughout for thread-safe DB operations
- Services registered as Scoped in Program.cs
- Modal components use EventCallback pattern for parent communication
- Equipment table has full sorting support on all columns
