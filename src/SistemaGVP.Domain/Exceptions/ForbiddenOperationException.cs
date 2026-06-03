namespace SistemaGVP.Domain.Exceptions;

public class ForbiddenOperationException : DomainException
{
    public string Operation { get; }
    public string Reason { get; }

    public ForbiddenOperationException(string operation, string reason)
        : base($"Operación '{operation}' no permitida: {reason}")
    {
        Operation = operation;
        Reason = reason;
    }
}
