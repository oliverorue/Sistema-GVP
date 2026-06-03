namespace SistemaGVP.Domain.Exceptions;

public class InactiveEntityException : DomainException
{
    public string EntityType { get; }
    public int EntityId { get; }
    public string EntityName { get; }

    public InactiveEntityException(string entityType, int entityId, string entityName)
        : base($"El {entityType} '{entityName}' está desactivado.")
    {
        EntityType = entityType;
        EntityId = entityId;
        EntityName = entityName;
    }
}
