# Project Brief: Equipment Inventory System (IInventory_Blazor)

## Overview
A web-based equipment inventory management system built with Blazor Server for tracking IT equipment and conducting inventory audits.

## Core Requirements

### Equipment Management
- Create, read, update, delete equipment records
- Inventory numbers (инвентарные номера) used as barcodes
- Equipment properties: name, type, manufacturer, model, inventory number, purchase date, status, workplace, assigned employee, notes
- Search and filter functionality by name, barcode, model, workplace, employee
- Print inventory number labels for equipment
- Split/duplicate equipment for multiple locations
- **Mobile-ready scanning**: Support for camera-based barcode scanning in browser

### Reference Data (Справочники)
- Employee management with soft delete
- Workplace management with responsible employee assignment
- Equipment-Workplace-Employee relationships

### Inventory Audit
- Create new inventory audits with auditor information
- Scan barcodes (manual input or scanner) to mark equipment as found
- Real-time statistics: total equipment, found, not found
- Mark/unmark equipment as found directly from lists
- Complete audits and track history
- Generate CSV reports with audit results

## Target Users
- IT administrators managing organizational equipment
- Inventory auditors conducting periodic equipment checks
- Russian-speaking users (UI is in Russian)

## Success Criteria
1. Equipment can be added with user-defined inventory numbers
2. Equipment can be assigned to workplaces and employees
3. Audits can track which equipment has been verified
4. Reports can be generated showing audit results
5. System provides clear visual feedback on audit progress
