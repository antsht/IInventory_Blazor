namespace InventoryApp.Services;

public class BarcodeService
{
    public string GenerateBarcode()
    {
        var timestamp = DateTime.UtcNow.Ticks.ToString("X").ToUpper();
        var random = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"EQ-{timestamp[^8..]}-{random}";
    }

    public string GenerateBarcodeHtml(string inventoryNumber, string name, string type, string model, string location, string assignedTo)
    {
        var binaryString = string.Join("", inventoryNumber.Select(c => Convert.ToString(c, 2).PadLeft(8, '0')));
        var bars = new System.Text.StringBuilder();
        
        const int barWidth = 2;
        const int height = 80;
        
        for (int i = 0; i < binaryString.Length; i++)
        {
            if (binaryString[i] == '1')
            {
                bars.Append($"<rect x=\"{i * barWidth}\" y=\"0\" width=\"{barWidth}\" height=\"{height}\" fill=\"black\"/>");
            }
        }

        var svgWidth = binaryString.Length * barWidth;
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <title>Инвентарный номер - {name}</title>
    <style>
        body {{
            margin: 0;
            padding: 20px;
            display: flex;
            flex-direction: column;
            align-items: center;
            font-family: Arial, sans-serif;
        }}
        .barcode-container {{
            text-align: center;
            page-break-after: always;
        }}
        h3 {{ margin: 10px 0; }}
        p {{ margin: 5px 0; }}
        svg {{ max-width: 100%; }}
        @media print {{
            body {{ padding: 10px; }}
        }}
    </style>
</head>
<body>
    <div class=""barcode-container"">
        <h3>{name}</h3>
        <p>{type} - {model}</p>
        <svg width=""{svgWidth}"" height=""{height + 30}"" xmlns=""http://www.w3.org/2000/svg"">
            {bars}
            <text x=""{svgWidth / 2}"" y=""{height + 20}"" text-anchor=""middle"" font-family=""monospace"" font-size=""14"">{inventoryNumber}</text>
        </svg>
        <p><strong>Инвентарный №:</strong> {inventoryNumber}</p>
        <p><strong>Местоположение:</strong> {(string.IsNullOrEmpty(location) ? "Не указано" : location)}</p>
        <p><strong>Назначено:</strong> {(string.IsNullOrEmpty(assignedTo) ? "Не назначено" : assignedTo)}</p>
    </div>
</body>
</html>";
    }
}


