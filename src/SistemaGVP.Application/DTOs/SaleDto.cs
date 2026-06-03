using SistemaGVP.Domain.Enums;

namespace SistemaGVP.Application.DTOs;

public class HeldSaleDto
{
    public int Id { get; set; }
    public string? CustomerName { get; set; }
    public List<SaleDetailDto> Items { get; set; } = new();
    public DateTime HeldAt { get; set; }
    public decimal SaleTotal { get; set; }
}

public class SaleDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "Completed";
    public string PaymentMethod { get; set; } = "Cash";
    public decimal CashAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SaleDetailDto> Items { get; set; } = new();
}

public class SaleDetailDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Subtotal { get; set; }
}

public class CreateSaleDto
{
    public int CompanyId { get; set; }
    public int UserId { get; set; }
    public int? CustomerId { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public decimal CashAmount { get; set; }
    public string? Notes { get; set; }
    public List<CreateSaleDetailDto> Items { get; set; } = new();
}

public class CreateSaleDetailDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
}
