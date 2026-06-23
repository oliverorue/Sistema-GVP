using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.Infrastructure.Services;

public class BackupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BackupBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(4);

    public BackupBackgroundService(IServiceProvider services, ILogger<BackupBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Backup automático iniciado — cada {Interval} horas", _interval.TotalHours);

        // Esperar 60 segundos al iniciar para que la app termine de arrancar
        await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);

        using var timer = new PeriodicTimer(_interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunBackupAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Backup automático detenido");
        }
    }

    private async Task RunBackupAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _services.CreateScope();
            var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();

            _logger.LogInformation("Iniciando backup automático...");
            var result = await backupService.CreateBackupAsync(
                companyId: 1,
                userId: 1);

            if (result.IsSuccess)
                _logger.LogInformation("Backup automático completado: {Path}", result.Data);
            else
                _logger.LogWarning("Backup automático falló: {Message}", result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en backup automático");
        }
    }
}
