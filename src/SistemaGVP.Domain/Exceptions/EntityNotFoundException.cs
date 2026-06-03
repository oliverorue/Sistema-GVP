namespace SistemaGVP.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public int EntityId { get; }

    public EntityNotFoundException(string entityType, int entityId)
        : base($"{entityType} con ID {entityId} no encontrado.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public EntityNotFoundException(string entityType, int entityId, string message)
        : base(message)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}
