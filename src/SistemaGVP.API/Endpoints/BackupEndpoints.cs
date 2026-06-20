using Microsoft.AspNetCore.Mvc;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class BackupEndpoints
{
    public static IEndpointRouteBuilder MapBackupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/backups").RequireAuthorization().RequireAuthorization("Admin");

        group.MapPost("/", async (IBackupService service, ICurrentUserService currentUser) =>
        {
            var result = await service.CreateBackupAsync(currentUser.CompanyId, currentUser.UserId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = new { filePath = result.Data }, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/", async (IBackupService service, ICurrentUserService currentUser) =>
        {
            var result = await service.GetBackupsAsync(currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/{fileName}/info", async (string fileName, IBackupService service, ICurrentUserService currentUser) =>
        {
            var backupsResult = await service.GetBackupsAsync(currentUser.CompanyId);
            if (!backupsResult.IsSuccess || backupsResult.Data == null)
                return Results.Ok(new { isSuccess = false, data = (object?)null, message = "No se pudo obtener la lista de backups.", errors = Array.Empty<string>() });

            var backupInfo = backupsResult.Data.FirstOrDefault(b => b.FileName == fileName);
            if (backupInfo == null)
                return Results.Ok(new { isSuccess = false, data = (object?)null, message = "Backup no encontrado.", errors = Array.Empty<string>() });

            var infoResult = await service.GetBackupInfoAsync(backupInfo.FilePath);
            return infoResult.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = infoResult.Data, message = infoResult.Message, errors = infoResult.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = infoResult.Message, errors = infoResult.Errors });
        });

        group.MapPost("/{fileName}/restore", async (string fileName, IBackupService service, ICurrentUserService currentUser) =>
        {
            var backupsResult = await service.GetBackupsAsync(currentUser.CompanyId);
            if (!backupsResult.IsSuccess || backupsResult.Data == null)
                return Results.Ok(new { isSuccess = false, data = (object?)null, message = "No se pudo obtener la lista de backups.", errors = Array.Empty<string>() });

            var backupInfo = backupsResult.Data.FirstOrDefault(b => b.FileName == fileName);
            if (backupInfo == null)
                return Results.Ok(new { isSuccess = false, data = (object?)null, message = "Backup no encontrado.", errors = Array.Empty<string>() });

            var result = await service.RestoreBackupAsync(backupInfo.FilePath, currentUser.CompanyId, currentUser.UserId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        return app;
    }
}
