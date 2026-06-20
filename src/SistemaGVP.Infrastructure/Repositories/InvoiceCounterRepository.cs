using Microsoft.EntityFrameworkCore;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Interfaces;
using SistemaGVP.Infrastructure.Data;

namespace SistemaGVP.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de contadores de facturas.
/// Usa una transacción con RepeatableRead para garantizar atomicidad
/// en la generación de números de factura consecutivos.
/// </summary>
public class InvoiceCounterRepository : IInvoiceCounterRepository
{
    private readonly AppDbContext _context;

    public InvoiceCounterRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetNextNumberAsync(int companyId, string datePrefix)
    {
        // Si ya hay una transacción activa (ej: desde SaleRepository.ExecuteWithSerializableTransactionAsync),
        // no creamos una nueva para evitar "The connection is already in a transaction".
        // Si no hay transacción activa, usamos RepeatableRead para atomicidad.
        if (_context.Database.CurrentTransaction != null)
        {
            // Ya estamos dentro de una transacción externa — ejecutar directamente
            var counter = await _context.InvoiceCounters
                .FirstOrDefaultAsync(c => c.CompanyId == companyId
                                       && c.DatePrefix == datePrefix);

            if (counter == null)
            {
                counter = new InvoiceCounter
                {
                    CompanyId = companyId,
                    DatePrefix = datePrefix,
                    LastNumber = 1
                };
                await _context.InvoiceCounters.AddAsync(counter);
            }
            else
            {
                counter.LastNumber++;
            }

            await _context.SaveChangesAsync();
            return counter.LastNumber;
        }
        else
        {
            // Sin transacción externa — usar estrategia atómica con RepeatableRead
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.RepeatableRead);

                try
                {
                    var counter = await _context.InvoiceCounters
                        .FirstOrDefaultAsync(c => c.CompanyId == companyId
                                               && c.DatePrefix == datePrefix);

                    if (counter == null)
                    {
                        counter = new InvoiceCounter
                        {
                            CompanyId = companyId,
                            DatePrefix = datePrefix,
                            LastNumber = 1
                        };
                        await _context.InvoiceCounters.AddAsync(counter);
                    }
                    else
                    {
                        counter.LastNumber++;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return counter.LastNumber;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
