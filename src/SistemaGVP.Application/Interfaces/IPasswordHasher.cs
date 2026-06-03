namespace SistemaGVP.Application.Interfaces;

/// <summary>
/// Interfaz para el servicio de hashing de contraseñas.
/// La implementación concreta (BCrypt) está en Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
