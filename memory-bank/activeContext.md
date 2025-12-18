# Active Context: Equipment Inventory System

## Current State
**Date**: December 17, 2025  
**Status**: Fully functional application with PDF report generation

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
- ✅ Navigation dropdown menus for Печать and Справочники
- ✅ **PDF Report Generation** (QuestPDF)
  - Список активного оборудования (алфавитная сортировка)
  - Список по рабочим местам (группировка, постраничный разрыв)
  - Печать ярлыков со штрих-кодами

## Recent Changes (December 17, 2025)
- **Implemented**: **Bulk Operations** in equipment list
  - Selection logic with checkboxes and "Select All"
  - Bulk actions bar with "Bulk Edit" and "Bulk Delete"
  - `BulkEditModal.razor` for updating multiple fields at once
  - Service methods for bulk updates/deletes
- **Added**: **Print menu (Печать)** in navigation with dropdown
  - `/reports/active` - Список активного оборудования (PDF)
  - `/reports/workplaces` - Список по рабочим местам (PDF)  
  - `/reports/labels` - Печать ярлыков со штрих-кодами (PDF)

## Current Focus
- Implementing **PWA support** and **Camera-based barcode scanning** for mobile devices.

## Next Steps
1. Create PWA manifest and service worker.
2. Integrate `html5-qrcode` library for camera-based scanning.
3. Update `BarcodeScannerModal.razor` to support camera scanning.
4. Test mobile experience.

## Active Decisions
- Using SQLite for simplicity (no external DB server needed)
- Russian-only UI (no i18n framework yet)
- Inventory number = Barcode (SerialNumber field stores it)
- SVG-based barcode rendering (simple binary representation)
- Standalone справочник pages instead of modal-only management
- QuestPDF Community license for PDF generation
- PDF reports filter only active equipment (status = "active")

## Known Considerations
- Blazor Server requires stable connection
- Popup blockers may interfere with barcode printing
- No offline capability
- No authentication (single-user assumed)
- DataSeedService reads from `data.csv` at startup if DB is empty
- QuestPDF requires Community license acknowledgment

## Architecture Notes
- `IDbContextFactory` pattern used throughout for thread-safe DB operations
- Services registered as Scoped in Program.cs
- Modal components use EventCallback pattern for parent communication
- All справочник tables have full sorting support
- Navigation uses two dropdown menus (Печать, Справочники)
- `PrintService` generates PDF reports with QuestPDF fluent API
