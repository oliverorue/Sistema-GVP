using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SistemaGVP.WPF.Services;

/// <summary>
/// Generador de códigos de barras numéricos de 13 dígitos (formato EAN-13)
/// con dígito verificador. Asigna códigos únicos basados en ID de producto.
/// </summary>
public class BarcodeGeneratorService
{
    private readonly ILogger<BarcodeGeneratorService>? _logger;

    // Prefijo de empresa: 770000 (código de país "770" para Colombia + "000" interno)
    // Puedes cambiarlo al prefijo que corresponda a tu empresa
    private const string CompanyPrefix = "770000";

    // Longitud total del código de barras (13 dígitos = EAN-13)
    private const int TotalLength = 13;

    public BarcodeGeneratorService(ILogger<BarcodeGeneratorService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Genera un código de barras de 13 dígitos único basado en un ID numérico.
    /// Formato: [CompanyPrefix (6)] + [ProductId padded (6)] + [CheckDigit (1)]
    /// </summary>
    /// <param name="productId">ID numérico del producto (se usan los últimos 6 dígitos)</param>
    /// <returns>Código de barras de 13 dígitos como string</returns>
    public string GenerateBarcode(int productId)
    {
        // Tomar los últimos 6 dígitos del ID (o padding con ceros a la izquierda)
        var idPart = (productId % 1_000_000).ToString("D6");

        // Construir los primeros 12 dígitos: prefix (6) + id (6)
        var codeWithoutCheckDigit = CompanyPrefix + idPart;

        // Calcular dígito verificador
        var checkDigit = CalculateEan13CheckDigit(codeWithoutCheckDigit);

        var barcode = codeWithoutCheckDigit + checkDigit;

        _logger?.LogInformation("Código de barras generado: {Barcode} para ID={ProductId}", barcode, productId);

        return barcode;
    }

    /// <summary>
    /// Genera un código de barras aleatorio único (cuando no hay ID aún, ej: producto nuevo no guardado).
    /// Usa timestamp + número aleatorio para evitar colisiones.
    /// </summary>
    public string GenerateRandomBarcode()
    {
        var random = new Random();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 1_000_000;
        var randomPart = random.Next(0, 999_999);
        var uniqueId = (timestamp + randomPart) % 1_000_000;

        var idPart = uniqueId.ToString("D6");
        var codeWithoutCheckDigit = CompanyPrefix + idPart;
        var checkDigit = CalculateEan13CheckDigit(codeWithoutCheckDigit);

        return codeWithoutCheckDigit + checkDigit;
    }

    /// <summary>
    /// Calcula el dígito verificador EAN-13.
    /// Algoritmo: suma de dígitos en posiciones pares e impares.
    /// </summary>
    private static string CalculateEan13CheckDigit(string code12)
    {
        if (code12.Length != 12)
            throw new ArgumentException("El código debe tener exactamente 12 dígitos", nameof(code12));

        var digits = code12.Select(c => int.Parse(c.ToString())).ToArray();

        // Suma de posiciones impares (1, 3, 5, 7, 9, 11) x 1
        var oddSum = 0;
        // Suma de posiciones pares (2, 4, 6, 8, 10, 12) x 3
        var evenSum = 0;

        for (int i = 0; i < digits.Length; i++)
        {
            if (i % 2 == 0) // Posición impar (índice 0 = posición 1)
                oddSum += digits[i];
            else // Posición par (índice 1 = posición 2)
                evenSum += digits[i];
        }

        var total = oddSum + (evenSum * 3);
        var checkDigit = (10 - (total % 10)) % 10;

        return checkDigit.ToString();
    }

    /// <summary>
    /// Verifica si un código de barras de 13 dígitos tiene un dígito verificador válido.
    /// </summary>
    public static bool IsValidBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode) || barcode.Length != 13 || !barcode.All(char.IsDigit))
            return false;

        var codeWithoutCheck = barcode.Substring(0, 12);
        var actualCheckDigit = barcode[12].ToString();
        var expectedCheckDigit = CalculateEan13CheckDigit(codeWithoutCheck);

        return actualCheckDigit == expectedCheckDigit;
    }

    /// <summary>
    /// Valida si un código de barras tiene el formato correcto (13 dígitos numéricos).
    /// </summary>
    public static bool IsValidBarcodeFormat(string barcode)
    {
        return !string.IsNullOrWhiteSpace(barcode) &&
               barcode.Length == 13 &&
               barcode.All(char.IsDigit);
    }
}
