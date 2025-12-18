using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;
using ZXing.SkiaSharp.Rendering;
using SkiaSharp;

namespace InventoryApp.Services;

public class BarcodeService
{
    public string GenerateBarcode()
    {
        var timestamp = DateTime.UtcNow.Ticks.ToString("X").ToUpper();
        var random = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"EQ-{timestamp[^8..]}-{random}";
    }

    /// <summary>
    /// Generates a real Code 128 barcode as base64 PNG image
    /// </summary>
    public string GenerateBarcodeImage(string data, int width = 300, int height = 80)
    {
        var writer = new BarcodeWriter<SKBitmap>
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 5,
                PureBarcode = true
            },
            Renderer = new SKBitmapRenderer()
        };

        using var bitmap = writer.Write(data);
        using var image = SKImage.FromBitmap(bitmap);
        using var encodedData = image.Encode(SKEncodedImageFormat.Png, 100);
        
        return Convert.ToBase64String(encodedData.ToArray());
    }

    /// <summary>
    /// Generates barcode as byte array for PDF embedding
    /// </summary>
    public byte[] GenerateBarcodeBytes(string data, int width = 200, int height = 50)
    {
        var writer = new BarcodeWriter<SKBitmap>
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 2,
                PureBarcode = true
            },
            Renderer = new SKBitmapRenderer()
        };

        using var bitmap = writer.Write(data);
        using var image = SKImage.FromBitmap(bitmap);
        using var encodedData = image.Encode(SKEncodedImageFormat.Png, 100);
        
        return encodedData.ToArray();
    }

    public string GenerateBarcodeHtml(string inventoryNumber, string name, string type, string location, string assignedTo)
    {
        string barcodeHtml;
        try
        {
            var barcodeBase64 = GenerateBarcodeImage(inventoryNumber);
            barcodeHtml = $"<img src=\"data:image/png;base64,{barcodeBase64}\" alt=\"{inventoryNumber}\" style=\"max-width: 100%; height: auto;\" />";
        }
        catch
        {
            // Fallback to text if barcode generation fails
            barcodeHtml = $"<div style='padding: 10px; border: 2px solid black; margin: 10px 0;'><span style='font-family: monospace; font-size: 20px; letter-spacing: 3px;'>{inventoryNumber}</span></div>";
        }

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
        h3 {{ margin: 10px 0; font-size: 14px; }}
        p {{ margin: 5px 0; font-size: 12px; }}
        .barcode-number {{ 
            font-family: 'Courier New', monospace; 
            font-size: 16px; 
            font-weight: bold;
            letter-spacing: 2px;
        }}
        @media print {{
            body {{ padding: 10px; }}
        }}
    </style>
</head>
<body>
    <div class=""barcode-container"">
        <h3>{name}</h3>
        <p>{type}</p>
        {barcodeHtml}
        <p class=""barcode-number"">{inventoryNumber}</p>
        <p>📍 {(string.IsNullOrEmpty(location) ? "Не указано" : location)}</p>
        <p>👤 {(string.IsNullOrEmpty(assignedTo) ? "Не назначено" : assignedTo)}</p>
    </div>
</body>
</html>";
    }
}
