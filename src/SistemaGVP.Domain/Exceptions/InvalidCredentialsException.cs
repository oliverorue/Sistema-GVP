namespace SistemaGVP.Domain.Exceptions;

public class InvalidCredentialsException : DomainException
{
    public string Username { get; }

    public InvalidCredentialsException(string username)
        : base($"Credenciales inválidas para el usuario '{username}'.")
    {
        Username = username;
    }
}
