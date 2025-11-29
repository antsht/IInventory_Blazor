# Project Brief: Equipment Inventory System (IInventory_Blazor)

## Overview
A web-based equipment inventory management system built with Blazor Server for tracking IT equipment and conducting inventory audits.

## Core Requirements

### Equipment Management
- Create, read, update, delete equipment records
- Auto-generated unique barcodes for each equipment item
- Equipment properties: name, type, manufacturer, model, serial number, purchase date, status, location, assigned user, notes
- Search and filter functionality by name, barcode, model, serial number, and equipment type
- Print barcode labels for equipment

### Inventory Audit
- Create new inventory audits with auditor information
- Scan barcodes (manual input or scanner) to mark equipment as found
- Real-time statistics: total equipment, found, not found
- Complete audits and track history
- Generate CSV reports with audit results

## Target Users
- IT administrators managing organizational equipment
- Inventory auditors conducting periodic equipment checks
- Russian-speaking users (UI is in Russian)

## Success Criteria
1. Equipment can be added with auto-generated barcodes
2. Audits can track which equipment has been verified
3. Reports can be generated showing audit results
4. System provides clear visual feedback on audit progress

