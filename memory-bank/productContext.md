# Product Context: Equipment Inventory System

## Why This Project Exists
Organizations need to track IT equipment for asset management, depreciation, and accountability. Manual tracking is error-prone and time-consuming. This system provides a digital solution for:
- Maintaining accurate equipment records
- Conducting periodic inventory audits
- Generating audit reports for compliance
- Managing equipment assignments to employees and workplaces

## Problems It Solves

### Equipment Tracking Challenges
- **Lost equipment**: Difficulty knowing what equipment exists and where it's located
- **No audit trail**: No systematic way to verify equipment presence
- **Manual processes**: Paper-based tracking is slow and error-prone
- **Reporting burden**: Creating inventory reports is time-consuming
- **Assignment tracking**: Who is responsible for which equipment

### Solution Approach
- **Centralized database**: All equipment in one searchable system
- **Barcode-based identification**: Quick, accurate equipment identification using inventory numbers
- **Structured audits**: Systematic process for verifying equipment
- **Automated reports**: One-click CSV report generation
- **Reference data management**: Employees and workplaces справочники with dedicated pages

## How It Should Work

### Equipment Lifecycle
1. **Add Equipment**: Create new equipment record with inventory number
2. **Assign Location**: Set workplace where equipment is located
3. **Assign Responsibility**: Set employee responsible for equipment
4. **Track Status**: Update equipment status (active, inactive, repair, disposed)
5. **Print Label**: Generate printable inventory label for physical equipment

### Reference Data Management
1. **Employees** (`/employees` page): 
   - Full sortable table view
   - Search by name, position, department
   - Add/edit with modal forms
   - Soft delete with usage check
   - Toggle to show inactive records
2. **Workplaces** (`/workplaces` page):
   - Full sortable table view  
   - Search by name, building, room
   - Assign responsible employee
   - Soft delete with usage check
   - Toggle to show inactive records
3. **Assignment**: Link employees to workplaces, auto-fill when selecting workplace

### Audit Workflow
1. **Start Audit**: Create new audit with auditor name
2. **Scan Equipment**: Use barcode scanner or manual entry to record found equipment
3. **Track Progress**: View real-time statistics on found vs not found
4. **Manual Override**: Click to mark items as found/not found
5. **Complete Audit**: Mark audit as complete
6. **Generate Report**: Download CSV with audit results

### Print Reports (PDF)
1. **Список активного оборудования**: Alphabetically sorted list of all active equipment
2. **Список по рабочим местам**: Equipment grouped by workplaces, each workplace on new page
3. **Печать ярлыков**: Barcode labels for physical equipment (multiple per page)

## User Experience Goals
- **Intuitive navigation**: Clear sections (Equipment, Audit, Справочники dropdown)
- **Fast data entry**: Forms with validation and smart defaults
- **Auto-fill**: Employee auto-selected from workplace
- **Visual feedback**: Clear status indicators and progress displays
- **Mobile-friendly**: Responsive design for tablet use during audits
- **Russian localization**: Full Russian language UI for target users

## Field Mappings (Technical → User Visible)
- `Barcode` → "Инвентарный номер" / "Инв. номер"
- `PurchaseDate` → "Дата добавления"
- `Workplace` → "Рабочее место"
- `Employee` → "Сотрудник" / "Ответственный сотрудник"
- `SerialNumber` → (deprecated, not shown in UI)

## Navigation Structure
- **Каталог оборудования** (`/`) - Main equipment list
- **Инвентаризация** (`/audit`) - Audit functionality
- **Печать** (dropdown):
  - **Список активного оборудования** (`/reports/active`) - PDF report
  - **Список по рабочим местам** (`/reports/workplaces`) - PDF report
  - **Печать ярлыков** (`/reports/labels`) - PDF barcode labels
- **Справочники** (dropdown):
  - **Сотрудники** (`/employees`) - Employee directory
  - **Рабочие места** (`/workplaces`) - Workplace directory
