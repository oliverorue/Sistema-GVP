namespace SistemaGVP.Application.DTOs;

public class SaleHistoryDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
    public decimal CashAmount { get; set; }
    public decimal ChangeAmount { get; set; }
}
