using Microsoft.EntityFrameworkCore;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Interfaces;
using SistemaGVP.Infrastructure.Data;

namespace SistemaGVP.Infrastructure.Repositories;

public class SaleRepository : BaseRepository<Sale>, ISaleRepository
{
    public SaleRepository(AppDbContext context) : base(context) { }

    public async Task<Sale?> GetByInvoiceNumberAsync(string invoiceNumber, int companyId)
    {
        return await _dbSet
            .Include(s => s.User)
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
            .FirstOrDefaultAsync(s => s.InvoiceNumber == invoiceNumber
                                   && s.CompanyId == companyId);
    }

    public async Task<IReadOnlyList<Sale>> GetByDateRangeAsync(int companyId, DateTime from, DateTime to)
    {
        return await _dbSet
            .Include(s => s.User)
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
            .Where(s => s.CompanyId == companyId
                     && s.CreatedAt >= from
                     && s.CreatedAt <= to)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Sale?> GetWithDetailsAsync(int saleId)
    {
        return await _dbSet
            .Include(s => s.User)
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
            .FirstOrDefaultAsync(s => s.Id == saleId);
    }

    public async Task<(IReadOnlyList<Sale> Items, int TotalCount)> GetSalesWithFilterAsync(
        int companyId,
        string? searchTerm,
        DateTime? startDate,
        DateTime? endDate,
        string? paymentMethod,
        int page,
        int pageSize)
    {
        var query = _dbSet
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
            .Where(s => s.CompanyId == companyId);

        // Filtro por rango de fechas
        if (startDate.HasValue)
            query = query.Where(s => s.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(s => s.CreatedAt <= endDate.Value);

        // Filtro por nombre de cliente
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(s => s.Customer != null && s.Customer.Name.Contains(searchTerm));

        // Filtro por método de pago
        if (!string.IsNullOrWhiteSpace(paymentMethod) && Enum.TryParse<Domain.Enums.PaymentMethod>(paymentMethod, out var pm))
            query = query.Where(s => s.PaymentMethod == pm);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Sale>> GetHeldSalesAsync(int companyId)
    {
        return await _dbSet
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
            .Where(s => s.CompanyId == companyId
                     && s.Status == Domain.Enums.SaleStatus.HeldSale)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<string> GetNextInvoiceNumberAsync(int companyId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"INV-{today:yyyyMMdd}-";

        var lastSale = await _dbSet
            .Where(s => s.CompanyId == companyId
                     && s.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(s => s.InvoiceNumber)
            .FirstOrDefaultAsync();

        if (lastSale == null)
            return $"{prefix}0001";

        var lastNumber = int.Parse(lastSale.InvoiceNumber[^4..]);
        return $"{prefix}{(lastNumber + 1):D4}";
    }

    public async Task<T> ExecuteWithSerializableTransactionAsync<T>(Func<Task<T>> action)
    {
        // Crear ExecutionStrategy (incluye retry automático para SQLite)
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

            try
            {
                var result = await action();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}
