using System;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace SistemaGVP.WPF.Services;

/// <summary>
/// Servicio para generar códigos QR localmente usando QRCoder.
/// No requiere conexión a internet ni dependencias de System.Drawing.
/// </summary>
public class QrCodeService
{
    private readonly ILogger<QrCodeService>? _logger;

    public QrCodeService(ILogger<QrCodeService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Genera un bitmap de código QR a partir del texto proporcionado.
    /// Usa PngByteQRCode para evitar dependencias de System.Drawing.
    /// </summary>
    /// <param name="text">Texto o URL a codificar en el QR.</param>
    /// <returns>Bitmap del código QR, o null si falla la generación.</returns>
    public BitmapImage? GenerateQrCode(string text)
    {
        System.Diagnostics.Debug.WriteLine($"[DIAG] QrCodeService.GenerateQrCode called. Text='{text}'");
        if (string.IsNullOrWhiteSpace(text))
        {
            System.Diagnostics.Debug.WriteLine("[DIAG] Text is null/empty, returning null");
            return null;
        }

        try
        {
            using var qrGenerator = new QRCodeGenerator();
            System.Diagnostics.Debug.WriteLine("[DIAG] QRCodeGenerator created");
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            System.Diagnostics.Debug.WriteLine("[DIAG] QRCodeData created");

            // PngByteQRCode genera bytes PNG directamente sin System.Drawing
            using var pngQrCode = new PngByteQRCode(qrCodeData);
            System.Diagnostics.Debug.WriteLine("[DIAG] PngByteQRCode created");
            var pngBytes = pngQrCode.GetGraphic(20, true);
            System.Diagnostics.Debug.WriteLine($"[DIAG] PNG bytes generated: {pngBytes.Length} bytes");

            using var ms = new MemoryStream(pngBytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            System.Diagnostics.Debug.WriteLine($"[DIAG] BitmapImage created");
            return bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DIAG] QR generation FAILED: {ex.GetType().Name}: {ex.Message}");
            _logger?.LogError(ex, "Error al generar código QR para: {Text}", text);
            return null;
        }
    }
}
