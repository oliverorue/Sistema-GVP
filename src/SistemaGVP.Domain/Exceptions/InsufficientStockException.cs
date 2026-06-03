namespace SistemaGVP.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public string ProductName { get; }
    public decimal CurrentStock { get; }
    public decimal RequestedQuantity { get; }

    public InsufficientStockException(string productName, decimal currentStock, decimal requestedQuantity)
        : base($"Stock insuficiente para '{productName}'. Disponible: {currentStock}, solicitado: {requestedQuantity}")
    {
        ProductName = productName;
        CurrentStock = currentStock;
        RequestedQuantity = requestedQuantity;
    }
}
