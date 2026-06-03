using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Infrastructure.Services;

/// <summary>
/// Servicio que mantiene el estado del usuario actual en la sesión.
/// Se asigna después del login exitoso y se inyecta en AppDbContext para auditoría.
/// Registrado como Singleton para mantener el estado de sesión.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Role == Domain.Enums.UserRole.Admin;

    public void SetCurrentUser(User user)
    {
        CurrentUser = user;
    }

    public void Clear()
    {
        CurrentUser = null;
    }

    public int UserId =>
        CurrentUser?.Id ?? throw new InvalidOperationException("No hay usuario autenticado");

    public string UserName =>
        CurrentUser?.FullName ?? throw new InvalidOperationException("No hay usuario autenticado");

    public int CompanyId =>
        CurrentUser?.CompanyId ?? throw new InvalidOperationException("No hay usuario autenticado");
}
