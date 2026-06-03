namespace SistemaGVP.Application.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    public string ActionDisplay => Action switch
    {
        "Create" => "Creación",
        "Update" => "Modificación",
        "Delete" => "Eliminación",
        "Login" => "Inicio de Sesión",
        "Logout" => "Cierre de Sesión",
        "CancelSale" => "Anulación de Venta",
        "BackupCreated" => "Backup Creado",
        "BackupRestored" => "Backup Restaurado",
        "ExportReport" => "Reporte Exportado",
        "LowStockAlert" => "Alerta de Stock Bajo",
        _ => Action
    };

    public string EntityNameDisplay => EntityName switch
    {
        "Product" => "Producto",
        "Category" => "Categoría",
        "Customer" => "Cliente",
        "Supplier" => "Proveedor",
        "Sale" => "Venta",
        "User" => "Usuario",
        "Company" => "Empresa",
        "InventoryMovement" => "Movimiento Inventario",
        _ => EntityName
    };

    public string Summary => $"{ActionDisplay} - {EntityNameDisplay} {(EntityId.HasValue ? $"#{EntityId}" : "")} por {UserName}";
}
