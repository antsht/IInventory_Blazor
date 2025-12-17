using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;
using System.Globalization;

namespace InventoryApp.Services;

public class DataSeedService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<DataSeedService> _logger;

    public DataSeedService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<DataSeedService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task SeedFromCsvAsync(string csvPath)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check if data already exists
        if (await context.Equipment.AnyAsync())
        {
            _logger.LogInformation("Database already contains data, skipping seed.");
            return;
        }

        if (!File.Exists(csvPath))
        {
            _logger.LogWarning("CSV file not found: {Path}", csvPath);
            return;
        }

        _logger.LogInformation("Starting data seed from {Path}", csvPath);

        var lines = await File.ReadAllLinesAsync(csvPath);
        var records = ParseCsv(lines);

        // Extract and create employees
        var employees = await CreateEmployeesAsync(context, records);
        _logger.LogInformation("Created {Count} employees", employees.Count);

        // Extract and create workplaces
        var workplaces = await CreateWorkplacesAsync(context, records, employees);
        _logger.LogInformation("Created {Count} workplaces", workplaces.Count);

        // Create equipment
        var equipmentCount = await CreateEquipmentAsync(context, records, employees, workplaces);
        _logger.LogInformation("Created {Count} equipment records", equipmentCount);

        _logger.LogInformation("Data seed completed successfully!");
    }

    private List<CsvRecord> ParseCsv(string[] lines)
    {
        var records = new List<CsvRecord>();
        
        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var fields = ParseCsvLine(line);
            if (fields.Count < 11) continue;

            records.Add(new CsvRecord
            {
                InventoryObjectId = fields[0],
                InventoryNumber = NormalizeString(fields[1]),
                Name = NormalizeString(fields[2]),
                DateAdded = ParseDate(fields[3]),
                WorkplaceId = fields[4],
                DepartmentId = fields[5],
                Notes = NormalizeString(fields[6]),
                WorkplaceName = NormalizeString(fields[7]),
                EmployeeName = NormalizeString(fields[8]),
                DepartmentName = NormalizeString(fields[9]),
                Status = NormalizeString(fields[10])
            });
        }

        return records;
    }

    private List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var currentField = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        fields.Add(currentField.ToString());
        return fields;
    }

    private string NormalizeString(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            return string.Empty;
        return value.Trim();
    }

    private DateOnly? ParseDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            return null;

        // Try parsing different date formats
        var formats = new[] { 
            "yyyy-MM-dd HH:mm:ss.fff", 
            "yyyy-MM-dd HH:mm:ss", 
            "yyyy-MM-dd",
            "dd.MM.yyyy"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(value.Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return DateOnly.FromDateTime(date);
            }
        }

        return null;
    }

    private async Task<Dictionary<string, Employee>> CreateEmployeesAsync(ApplicationDbContext context, List<CsvRecord> records)
    {
        var employees = new Dictionary<string, Employee>(StringComparer.OrdinalIgnoreCase);

        // Get unique employee-department combinations
        var uniqueEmployees = records
            .Where(r => !string.IsNullOrEmpty(r.EmployeeName) && 
                        r.EmployeeName != "Списание" && 
                        r.EmployeeName != "Комендант")
            .GroupBy(r => r.EmployeeName)
            .Select(g => new { Name = g.Key, Department = g.First().DepartmentName })
            .ToList();

        // Add special employees for "Списание" and "Комендант" departments
        var specialEmployees = records
            .Where(r => r.EmployeeName == "Списание" || r.EmployeeName == "Комендант")
            .Select(r => r.EmployeeName)
            .Distinct()
            .ToList();

        foreach (var emp in uniqueEmployees)
        {
            var employee = new Employee
            {
                FullName = emp.Name,
                Department = emp.Department,
                Position = "",
                IsActive = true
            };
            context.Employees.Add(employee);
            employees[emp.Name] = employee;
        }

        // Create special "employees" for Списание and Комендант
        foreach (var specialName in specialEmployees)
        {
            if (!employees.ContainsKey(specialName))
            {
                var employee = new Employee
                {
                    FullName = specialName,
                    Department = specialName,
                    Position = "",
                    IsActive = specialName != "Списание" // Списание is inactive
                };
                context.Employees.Add(employee);
                employees[specialName] = employee;
            }
        }

        await context.SaveChangesAsync();
        return employees;
    }

    private async Task<Dictionary<string, Workplace>> CreateWorkplacesAsync(
        ApplicationDbContext context, 
        List<CsvRecord> records,
        Dictionary<string, Employee> employees)
    {
        var workplaces = new Dictionary<string, Workplace>(StringComparer.OrdinalIgnoreCase);

        // Get unique workplaces with their primary employee
        var uniqueWorkplaces = records
            .Where(r => !string.IsNullOrEmpty(r.WorkplaceName))
            .GroupBy(r => r.WorkplaceName)
            .Select(g => new 
            { 
                Name = g.Key, 
                EmployeeName = g.First().EmployeeName,
                Department = g.First().DepartmentName
            })
            .ToList();

        foreach (var wp in uniqueWorkplaces)
        {
            Employee? employee = null;
            if (!string.IsNullOrEmpty(wp.EmployeeName) && employees.TryGetValue(wp.EmployeeName, out var emp))
            {
                employee = emp;
            }

            // Extract room number from workplace name (e.g., "Рабочее место №10" -> "10")
            var roomNumber = "";
            var match = System.Text.RegularExpressions.Regex.Match(wp.Name, @"№(\d+)");
            if (match.Success)
            {
                roomNumber = match.Groups[1].Value;
            }

            var workplace = new Workplace
            {
                Name = wp.Name,
                Room = roomNumber,
                Description = wp.Department,
                EmployeeId = employee?.Id,
                IsActive = wp.EmployeeName != "Списание"
            };
            context.Workplaces.Add(workplace);
            workplaces[wp.Name] = workplace;
        }

        await context.SaveChangesAsync();
        return workplaces;
    }

    private async Task<int> CreateEquipmentAsync(
        ApplicationDbContext context,
        List<CsvRecord> records,
        Dictionary<string, Employee> employees,
        Dictionary<string, Workplace> workplaces)
    {
        var processedBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var count = 0;

        foreach (var record in records)
        {
            // Determine inventory number (barcode)
            var barcode = !string.IsNullOrEmpty(record.InventoryNumber) 
                ? record.InventoryNumber 
                : $"AUTO-{record.InventoryObjectId}";

            // Skip duplicates (same barcode with same notes typically means same physical item)
            var uniqueKey = $"{barcode}|{record.Notes}";
            if (processedBarcodes.Contains(uniqueKey))
                continue;
            processedBarcodes.Add(uniqueKey);

            // Determine equipment type from name
            var equipmentType = DetermineEquipmentType(record.Name);

            // Determine status
            var status = record.Status.Contains("Списан", StringComparison.OrdinalIgnoreCase) 
                ? "disposed" 
                : "active";

            // Get workplace and employee
            Workplace? workplace = null;
            Employee? employee = null;

            if (!string.IsNullOrEmpty(record.WorkplaceName) && workplaces.TryGetValue(record.WorkplaceName, out var wp))
            {
                workplace = wp;
            }

            if (!string.IsNullOrEmpty(record.EmployeeName) && employees.TryGetValue(record.EmployeeName, out var emp))
            {
                employee = emp;
            }

            // Build equipment name with notes if present
            var name = record.Name;
            if (name.Length > 200)
                name = name.Substring(0, 197) + "...";

            var notes = record.Notes;
            if (notes.Length > 1000)
                notes = notes.Substring(0, 997) + "...";

            var equipment = new Equipment
            {
                Barcode = barcode.Length > 50 ? barcode.Substring(0, 50) : barcode,
                SerialNumber = barcode.Length > 100 ? barcode.Substring(0, 100) : barcode,
                Name = name,
                Type = equipmentType,
                Status = status,
                PurchaseDate = record.DateAdded,
                WorkplaceId = workplace?.Id,
                EmployeeId = employee?.Id,
                Notes = notes
            };

            context.Equipment.Add(equipment);
            count++;
        }

        await context.SaveChangesAsync();
        return count;
    }

    private string DetermineEquipmentType(string name)
    {
        var nameLower = name.ToLowerInvariant();

        if (nameLower.Contains("ноутбук"))
            return "Laptop";

        if (nameLower.Contains("монитор"))
            return "Monitor";

        if (nameLower.Contains("принтер"))
            return "Printer";

        if (nameLower.Contains("сканер"))
            return "Scanner";

        if (nameLower.Contains("сервер"))
            return "Server";

        if (nameLower.Contains("коммутатор") || 
            nameLower.Contains("переключатель") || 
            nameLower.Contains("роутер") ||
            nameLower.Contains("маршрутизатор") ||
            nameLower.Contains("switch") ||
            nameLower.Contains("d-link") ||
            nameLower.Contains("tp-link") ||
            nameLower.Contains("netgear"))
            return "Network";

        if (nameLower.Contains("пк") || 
            nameLower.Contains("персональный компьютер") || 
            nameLower.Contains("системный блок") ||
            nameLower.Contains("неттоп") ||
            nameLower.Contains("core i") ||
            nameLower.Contains("intel"))
            return "PC";

        if (nameLower.Contains("мфу"))
            return "MFP";

        return "Other";
    }

    private class CsvRecord
    {
        public string InventoryObjectId { get; set; } = "";
        public string InventoryNumber { get; set; } = "";
        public string Name { get; set; } = "";
        public DateOnly? DateAdded { get; set; }
        public string WorkplaceId { get; set; } = "";
        public string DepartmentId { get; set; } = "";
        public string Notes { get; set; } = "";
        public string WorkplaceName { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public string Status { get; set; } = "";
    }
}

