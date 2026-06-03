namespace SistemaGVP.Domain.Interfaces;

/// <summary>
/// Unit of Work para manejar transacciones atómicas.
/// </summary>
public interface IUnitOfWork
{
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}
