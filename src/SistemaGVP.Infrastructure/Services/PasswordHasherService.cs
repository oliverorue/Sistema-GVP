using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.Infrastructure.Services;

/// <summary>
/// Servicio para hashing y verificación de contraseñas utilizando BCrypt.
/// </summary>
public class PasswordHasherService : IPasswordHasher
{
    /// <summary>
    /// Genera el hash de una contraseña en texto plano.
    /// </summary>
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Verifica si una contraseña en texto plano coincide con el hash almacenado.
    /// </summary>
    public bool Verify(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
