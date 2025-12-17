using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InventoryApp.Services;

public class PrintService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

    static PrintService()
    {
        // Configure QuestPDF license (Community license for open-source projects)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Получить активное оборудование для печати
    /// </summary>
    private async Task<List<Equipment>> GetActiveEquipmentAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Equipment
            .Include(e => e.Workplace)
            .Include(e => e.Employee)
            .Where(e => e.Status == "active")
            .ToListAsync();
    }

    /// <summary>
    /// Генерация PDF: Список активного оборудования (сортировка по алфавиту)
    /// </summary>
    public async Task<byte[]> GenerateActiveEquipmentListAsync()
    {
        var equipment = await GetActiveEquipmentAsync();
        var sortedEquipment = equipment.OrderBy(e => e.Name).ThenBy(e => e.Barcode).ToList();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(ComposeHeader("Список активного оборудования"));

                page.Content().Element(content =>
                {
                    content.Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);   // №
                            columns.RelativeColumn(3);    // Название
                            columns.RelativeColumn(1.5f); // Тип
                            columns.RelativeColumn(2);    // Инв. номер
                            columns.RelativeColumn(2);    // Рабочее место
                            columns.RelativeColumn(2);    // Сотрудник
                        });

                        // Header row
                        table.Header(header =>
                        {
                            ComposeTableHeaderCell(header.Cell(), "№");
                            ComposeTableHeaderCell(header.Cell(), "Название");
                            ComposeTableHeaderCell(header.Cell(), "Тип");
                            ComposeTableHeaderCell(header.Cell(), "Инв. номер");
                            ComposeTableHeaderCell(header.Cell(), "Рабочее место");
                            ComposeTableHeaderCell(header.Cell(), "Сотрудник");
                        });

                        // Data rows
                        int rowNum = 1;
                        foreach (var item in sortedEquipment)
                        {
                            var isAlternate = rowNum % 2 == 0;
                            
                            ComposeTableCell(table.Cell(), rowNum.ToString(), isAlternate);
                            ComposeTableCell(table.Cell(), item.Name, isAlternate);
                            ComposeTableCell(table.Cell(), GetTypeName(item.Type), isAlternate);
                            ComposeTableCell(table.Cell(), item.Barcode, isAlternate);
                            ComposeTableCell(table.Cell(), item.Workplace?.Name ?? "-", isAlternate);
                            ComposeTableCell(table.Cell(), item.Employee?.FullName ?? "-", isAlternate);
                            
                            rowNum++;
                        }
                    });
                });

                page.Footer().Element(ComposeFooter(sortedEquipment.Count));
            });
        }).GeneratePdf();
    }

    /// <summary>
    /// Генерация PDF: Список по рабочим местам (группировка, новое место - новая страница)
    /// </summary>
    public async Task<byte[]> GenerateEquipmentByWorkplacesAsync()
    {
        var equipment = await GetActiveEquipmentAsync();
        
        // Group by workplace (null workplace goes to "Не назначено")
        var grouped = equipment
            .GroupBy(e => e.Workplace)
            .OrderBy(g => g.Key?.Name ?? "яяя") // "яяя" to put "Не назначено" last
            .ToList();

        return Document.Create(container =>
        {
            foreach (var group in grouped)
            {
                var workplaceName = group.Key?.Name ?? "Не назначено";
                var workplaceLocation = group.Key != null 
                    ? $"{group.Key.Building}, этаж {group.Key.Floor}, каб. {group.Key.Room}".Trim(' ', ',')
                    : "";
                var sortedItems = group.OrderBy(e => e.Name).ToList();

                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Список оборудования по рабочим местам")
                                .FontSize(16).Bold();
                            row.ConstantItem(100).AlignRight().Text(DateTime.Now.ToString("dd.MM.yyyy"))
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
                        });
                        
                        column.Item().PaddingTop(10).Column(c =>
                        {
                            c.Item().Text(workplaceName).FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                            if (!string.IsNullOrEmpty(workplaceLocation))
                            {
                                c.Item().Text(workplaceLocation).FontSize(10).FontColor(Colors.Grey.Darken1);
                            }
                        });
                        
                        column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().Element(content =>
                    {
                        content.Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);   // №
                                columns.RelativeColumn(3);    // Название
                                columns.RelativeColumn(1.5f); // Тип
                                columns.RelativeColumn(2);    // Инв. номер
                                columns.RelativeColumn(2.5f); // Сотрудник
                            });

                            table.Header(header =>
                            {
                                ComposeTableHeaderCell(header.Cell(), "№");
                                ComposeTableHeaderCell(header.Cell(), "Название");
                                ComposeTableHeaderCell(header.Cell(), "Тип");
                                ComposeTableHeaderCell(header.Cell(), "Инв. номер");
                                ComposeTableHeaderCell(header.Cell(), "Сотрудник");
                            });

                            int rowNum = 1;
                            foreach (var item in sortedItems)
                            {
                                var isAlternate = rowNum % 2 == 0;
                                
                                ComposeTableCell(table.Cell(), rowNum.ToString(), isAlternate);
                                ComposeTableCell(table.Cell(), item.Name, isAlternate);
                                ComposeTableCell(table.Cell(), GetTypeName(item.Type), isAlternate);
                                ComposeTableCell(table.Cell(), item.Barcode, isAlternate);
                                ComposeTableCell(table.Cell(), item.Employee?.FullName ?? "-", isAlternate);
                                
                                rowNum++;
                            }
                        });
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text($"Всего на рабочем месте: {sortedItems.Count}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                        row.ConstantItem(100).AlignRight().Text(text =>
                        {
                            text.Span("Стр. ").FontSize(9).FontColor(Colors.Grey.Darken1);
                            text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Darken1);
                            text.Span(" из ").FontSize(9).FontColor(Colors.Grey.Darken1);
                            text.TotalPages().FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            }
        }).GeneratePdf();
    }

    /// <summary>
    /// Генерация PDF: Ярлыки со штрих-кодами (несколько на странице, группировка по рабочим местам)
    /// </summary>
    public async Task<byte[]> GenerateLabelsAsync()
    {
        var equipment = await GetActiveEquipmentAsync();
        
        // Group by workplace and sort
        var grouped = equipment
            .GroupBy(e => e.Workplace)
            .OrderBy(g => g.Key?.Name ?? "яяя")
            .ToList();

        // Flatten to list with workplace info, sorted by workplace then by name
        var allItems = grouped
            .SelectMany(g => g.OrderBy(e => e.Name).Select(e => new { Equipment = e, Workplace = g.Key }))
            .ToList();

        const int labelsPerRow = 2;
        const int rowsPerPage = 5;
        const int labelsPerPage = labelsPerRow * rowsPerPage;

        return Document.Create(container =>
        {
            var pages = (int)Math.Ceiling(allItems.Count / (double)labelsPerPage);
            
            for (int pageIdx = 0; pageIdx < pages; pageIdx++)
            {
                var pageItems = allItems.Skip(pageIdx * labelsPerPage).Take(labelsPerPage).ToList();
                
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Text("Ярлыки оборудования")
                            .FontSize(12).Bold();
                        row.ConstantItem(80).AlignRight().Text(text =>
                        {
                            text.Span("Стр. ").FontSize(9);
                            text.CurrentPageNumber().FontSize(9);
                        });
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        foreach (var item in pageItems)
                        {
                            table.Cell().Padding(5).Element(cell => ComposeLabel(cell, item.Equipment, item.Workplace));
                        }

                        // Fill empty cells if needed to maintain layout
                        var emptyCount = labelsPerPage - pageItems.Count;
                        for (int i = 0; i < emptyCount % labelsPerRow; i++)
                        {
                            table.Cell();
                        }
                    });
                });
            }
        }).GeneratePdf();
    }

    // Helper methods

    private static Action<IContainer> ComposeHeader(string title)
    {
        return container =>
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text(title).FontSize(16).Bold();
                    row.ConstantItem(100).AlignRight().Text(DateTime.Now.ToString("dd.MM.yyyy"))
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                });
                column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });
        };
    }

    private static Action<IContainer> ComposeFooter(int totalCount)
    {
        return container =>
        {
            container.Row(row =>
            {
                row.RelativeItem().Text($"Всего: {totalCount}")
                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                row.ConstantItem(100).AlignRight().Text(text =>
                {
                    text.Span("Стр. ").FontSize(9).FontColor(Colors.Grey.Darken1);
                    text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Darken1);
                    text.Span(" из ").FontSize(9).FontColor(Colors.Grey.Darken1);
                    text.TotalPages().FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });
        };
    }

    private static void ComposeTableHeaderCell(IContainer cell, string text)
    {
        cell.Background(Colors.Grey.Lighten2)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten1)
            .Padding(5)
            .Text(text)
            .Bold()
            .FontSize(9);
    }

    private static void ComposeTableCell(IContainer cell, string text, bool isAlternate)
    {
        var container = isAlternate 
            ? cell.Background(Colors.Grey.Lighten4) 
            : cell;
        
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten1)
            .Padding(5)
            .Text(text)
            .FontSize(9);
    }

    private static void ComposeLabel(IContainer container, Equipment equipment, Workplace? workplace)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Medium)
            .Background(Colors.White)
            .Padding(10)
            .Column(column =>
            {
                column.Spacing(3);

                // Equipment name
                column.Item().Text(equipment.Name)
                    .FontSize(10)
                    .Bold()
                    .LineHeight(1.1f);

                // Type
                column.Item().Text(GetTypeName(equipment.Type))
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);

                // Barcode visualization
                column.Item().PaddingVertical(5).Element(barcodeContainer =>
                {
                    ComposeBarcodeVisualization(barcodeContainer, equipment.Barcode);
                });

                // Inventory number
                column.Item().AlignCenter().Text(equipment.Barcode)
                    .FontSize(10)
                    .FontFamily("Courier New");

                // Separator
                column.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

                // Workplace info
                column.Item().Text($"📍 {workplace?.Name ?? "Не назначено"}")
                    .FontSize(8);

                // Employee info
                if (equipment.Employee != null)
                {
                    column.Item().Text($"👤 {equipment.Employee.FullName}")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Darken1);
                }
            });
    }

    private static void ComposeBarcodeVisualization(IContainer container, string barcode)
    {
        // Convert barcode to binary representation for simple visualization
        var binaryString = string.Join("", barcode.Select(c => Convert.ToString(c, 2).PadLeft(8, '0')));
        
        // Use Row layout with narrow columns for barcode visualization
        container.Height(35).Row(row =>
        {
            const float barWidth = 1.5f;
            
            foreach (char bit in binaryString)
            {
                if (bit == '1')
                {
                    row.ConstantItem(barWidth).Background(Colors.Black);
                }
                else
                {
                    row.ConstantItem(barWidth).Background(Colors.White);
                }
            }
        });
    }

    private static string GetTypeName(string typeCode)
    {
        return EquipmentTypes.All.TryGetValue(typeCode, out var name) ? name : typeCode;
    }
}

