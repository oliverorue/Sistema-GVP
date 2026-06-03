using Microsoft.Extensions.Logging;

namespace SistemaGVP.Application.Common;

public static class LoggingExtensions
{
    public static void LogEntityCreated(this ILogger logger, string entityType, object id, string name)
    {
        logger.LogInformation("{EntityType} creado: {Id} | {Name}", entityType, id, name);
    }

    public static void LogEntityUpdated(this ILogger logger, string entityType, object id, string name)
    {
        logger.LogInformation("{EntityType} actualizado: {Id} | {Name}", entityType, id, name);
    }

    public static void LogEntityDeleted(this ILogger logger, string entityType, object id, string? name = null)
    {
        if (name is not null)
            logger.LogInformation("{EntityType} desactivado: {Id} | {Name}", entityType, id, name);
        else
            logger.LogInformation("{EntityType} desactivado: {Id}", entityType, id);
    }

    public static void LogOperationSuccess(this ILogger logger, string operation, params object?[] args)
    {
        logger.LogInformation("Operación '{Operation}' exitosa.", operation);
    }

    public static void LogRepositoryError(this ILogger logger, Exception ex, string operation, params object?[] args)
    {
        logger.LogError(ex, "Error en {Operation}: {Message}", operation, ex.Message);
    }

    public static IDisposable? BeginOperationScope(this ILogger logger, string operation, params (string Key, object? Value)[] properties)
    {
        var state = properties.ToDictionary(p => p.Key, p => p.Value);
        state["Operation"] = operation;
        return logger.BeginScope(state);
    }
}
