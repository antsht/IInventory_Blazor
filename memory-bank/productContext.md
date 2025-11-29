# Product Context: Equipment Inventory System

## Why This Project Exists
Organizations need to track IT equipment for asset management, depreciation, and accountability. Manual tracking is error-prone and time-consuming. This system provides a digital solution for:
- Maintaining accurate equipment records
- Conducting periodic inventory audits
- Generating audit reports for compliance

## Problems It Solves

### Equipment Tracking Challenges
- **Lost equipment**: Difficulty knowing what equipment exists and where it's located
- **No audit trail**: No systematic way to verify equipment presence
- **Manual processes**: Paper-based tracking is slow and error-prone
- **Reporting burden**: Creating inventory reports is time-consuming

### Solution Approach
- **Centralized database**: All equipment in one searchable system
- **Barcode-based identification**: Quick, accurate equipment identification
- **Structured audits**: Systematic process for verifying equipment
- **Automated reports**: One-click CSV report generation

## How It Should Work

### Equipment Lifecycle
1. **Add Equipment**: Create new equipment record → system generates unique barcode
2. **Print Barcode**: Generate printable barcode label for physical equipment
3. **Track Status**: Update equipment status (active, inactive, repair, disposed)
4. **Assign Location**: Record where equipment is located and who is responsible

### Audit Workflow
1. **Start Audit**: Create new audit with auditor name
2. **Scan Equipment**: Use barcode scanner or manual entry to record found equipment
3. **Track Progress**: View real-time statistics on found vs not found
4. **Complete Audit**: Mark audit as complete
5. **Generate Report**: Download CSV with audit results

## User Experience Goals
- **Intuitive navigation**: Clear two-section interface (Equipment, Audit)
- **Fast data entry**: Forms with validation and smart defaults
- **Visual feedback**: Clear status indicators and progress displays
- **Mobile-friendly**: Responsive design for tablet use during audits
- **Russian localization**: Full Russian language UI for target users

