namespace SistemaGVP.Domain.Exceptions;

public class InvalidBarcodeException : DomainException
{
    public string Barcode { get; }

    public InvalidBarcodeException(string barcode)
        : base($"El código de barras '{barcode}' no es válido.")
    {
        Barcode = barcode;
    }
}
