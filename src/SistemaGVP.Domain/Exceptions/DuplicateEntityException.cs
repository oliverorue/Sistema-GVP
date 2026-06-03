namespace SistemaGVP.Domain.Exceptions;

public class DuplicateEntityException : DomainException
{
    public string EntityType { get; }
    public string Field { get; }
    public string Value { get; }

    public DuplicateEntityException(string entityType, string field, string value)
        : base($"Ya existe un {entityType} con {field} '{value}'.")
    {
        EntityType = entityType;
        Field = field;
        Value = value;
    }
}
