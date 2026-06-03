using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Interfaces;
using SistemaGVP.Infrastructure.Data;

namespace SistemaGVP.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly AppDbContext _context;
    private readonly IRepository<Company> _companyRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BackupService> _logger;

    public BackupService(
        AppDbContext context,
        IRepository<Company> companyRepository,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        ILogger<BackupService> logger)
    {
        _context = context;
        _companyRepository = companyRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private string GetDatabasePath()
    {
        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("No se pudo obtener la cadena de conexión.");

        // Parse SQLite connection string to get the file path
        // Format: "Data Source=/path/to/database.db"
        const string dataSourcePrefix = "Data Source=";
        var dataSourceIndex = connectionString.IndexOf(dataSourcePrefix, StringComparison.OrdinalIgnoreCase);
        if (dataSourceIndex < 0)
            throw new InvalidOperationException("Cadena de conexión no válida para SQLite.");

        var dbPath = connectionString[(dataSourceIndex + dataSourcePrefix.Length)..].Trim();
        if (string.IsNullOrEmpty(dbPath))
            throw new InvalidOperationException("No se pudo determinar la ruta de la base de datos.");

        return dbPath;
    }

    private string GetBackupDirectory(string dbPath)
    {
        var dbDirectory = Path.GetDirectoryName(dbPath) ?? ".";
        var backupDir = Path.Combine(dbDirectory, "Backups");
        Directory.CreateDirectory(backupDir);
        return backupDir;
    }

    public async Task<ServiceResult<string>> CreateBackupAsync(int companyId, int userId)
    {
        try
        {
            var dbPath = GetDatabasePath();
            var backupDir = GetBackupDirectory(dbPath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"GVP_Backup_{companyId}_{timestamp}.db";
            var backupPath = Path.Combine(backupDir, fileName);

            // Copy the database file
            await Task.Run(() => File.Copy(dbPath, backupPath, overwrite: false));

            // Calculate SHA256 hash
            string hash;
            await using (var stream = File.OpenRead(backupPath))
            {
                var sha256 = SHA256.Create();
                var hashBytes = await sha256.ComputeHashAsync(stream);
                hash = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
            }

            // Get metadata
            var company = await _companyRepository.GetByIdAsync(companyId);
            var user = await _userRepository.GetByIdAsync(userId);
            var fileInfo = new FileInfo(backupPath);

            // Create metadata file
            var metadata = new BackupMetadata
            {
                CompanyId = companyId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                HashSha256 = hash,
                FileSizeBytes = fileInfo.Length,
                OriginalDbPath = dbPath,
                CompanyName = company?.Name ?? "Desconocida",
                CreatedByUser = user?.FullName ?? "Desconocido"
            };

            var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            var metadataPath = Path.Combine(backupDir, $"{Path.GetFileNameWithoutExtension(fileName)}.metadata");
            await File.WriteAllTextAsync(metadataPath, metadataJson);

            _logger.LogInformation(
                "Backup creado: {FileName} | Empresa: {CompanyId} | Usuario: {UserId} | Hash: {Hash}",
                fileName, companyId, userId, hash);

            return ServiceResult<string>.Success(backupPath, $"Backup creado exitosamente: {fileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear backup para empresa {CompanyId}", companyId);
            return ServiceResult<string>.Failure($"Error al crear backup: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> RestoreBackupAsync(string backupFilePath, int companyId, int userId)
    {
        try
        {
            if (!File.Exists(backupFilePath))
                return ServiceResult<bool>.Failure("El archivo de backup no existe.");

            var backupDir = Path.GetDirectoryName(backupFilePath) ?? ".";
            var metadataPath = Path.Combine(backupDir, $"{Path.GetFileNameWithoutExtension(backupFilePath)}.metadata");

            // Verify integrity if metadata exists
            if (File.Exists(metadataPath))
            {
                var metadataJson = await File.ReadAllTextAsync(metadataPath);
                var metadata = JsonSerializer.Deserialize<BackupMetadata>(metadataJson);

                if (metadata != null && !string.IsNullOrEmpty(metadata.HashSha256))
                {
                    string currentHash;
                    await using (var stream = File.OpenRead(backupFilePath))
                    {
                        var sha256 = SHA256.Create();
                        var hashBytes = await sha256.ComputeHashAsync(stream);
                        currentHash = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
                    }

                    if (currentHash != metadata.HashSha256)
                    {
                        _logger.LogWarning(
                            "Hash del backup no coincide. Archivo: {File} | Esperado: {Expected} | Actual: {Actual}",
                            backupFilePath, metadata.HashSha256, currentHash);
                        return ServiceResult<bool>.Failure(
                            "La integridad del backup no pudo ser verificada. El archivo podría estar corrupto.");
                    }
                }
            }

            var dbPath = GetDatabasePath();

            // Auto-backup current state before restoring
            var currentTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var preRestoreBackupDir = GetBackupDirectory(dbPath);
            var preRestoreBackupPath = Path.Combine(
                preRestoreBackupDir,
                $"PreRestore_Backup_{companyId}_{currentTimestamp}.db");

            await Task.Run(() => File.Copy(dbPath, preRestoreBackupPath, overwrite: false));
            _logger.LogInformation(
                "Backup pre-restauración creado: {File}", preRestoreBackupPath);

            // Replace the database file with the backup
            await Task.Run(() => File.Copy(backupFilePath, dbPath, overwrite: true));

            _logger.LogInformation(
                "Backup restaurado: {File} | Empresa: {CompanyId} | Usuario: {UserId}",
                backupFilePath, companyId, userId);

            return ServiceResult<bool>.Success(true, 
                "Backup restaurado exitosamente. Es necesario reiniciar la aplicación para aplicar los cambios.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al restaurar backup {File}", backupFilePath);
            return ServiceResult<bool>.Failure($"Error al restaurar backup: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<BackupInfoDto>>> GetBackupsAsync(int companyId)
    {
        try
        {
            var dbPath = GetDatabasePath();
            var backupDir = GetBackupDirectory(dbPath);

            if (!Directory.Exists(backupDir))
                return ServiceResult<List<BackupInfoDto>>.Success(new List<BackupInfoDto>());

            var backupFiles = Directory.GetFiles(backupDir, "GVP_Backup_*.db")
                .OrderByDescending(f => f)
                .ToList();

            var backups = new List<BackupInfoDto>();

            foreach (var file in backupFiles)
            {
                var info = await GetBackupInfoInternalAsync(file);
                if (info != null && info.CompanyName != null)
                {
                    // Filter by company if metadata is available
                    backups.Add(info);
                }
                else
                {
                    // Fallback: add basic info from filename
                    var fileInfo = new FileInfo(file);
                    backups.Add(new BackupInfoDto
                    {
                        FileName = fileInfo.Name,
                        FilePath = fileInfo.FullName,
                        FileSizeBytes = fileInfo.Length,
                        CreatedAt = fileInfo.CreationTimeUtc
                    });
                }
            }

            return ServiceResult<List<BackupInfoDto>>.Success(backups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar backups para empresa {CompanyId}", companyId);
            return ServiceResult<List<BackupInfoDto>>.Failure("Error al listar backups.");
        }
    }

    public async Task<ServiceResult<BackupInfoDto>> GetBackupInfoAsync(string backupFilePath)
    {
        try
        {
            var info = await GetBackupInfoInternalAsync(backupFilePath);
            if (info == null)
                return ServiceResult<BackupInfoDto>.Failure("No se pudo leer la información del backup.");

            return ServiceResult<BackupInfoDto>.Success(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener info del backup {File}", backupFilePath);
            return ServiceResult<BackupInfoDto>.Failure("Error al obtener información del backup.");
        }
    }

    private async Task<BackupInfoDto?> GetBackupInfoInternalAsync(string backupFilePath)
    {
        try
        {
            var fileInfo = new FileInfo(backupFilePath);
            var backupDir = Path.GetDirectoryName(backupFilePath) ?? ".";
            var metadataPath = Path.Combine(backupDir, $"{Path.GetFileNameWithoutExtension(backupFilePath)}.metadata");

            var dto = new BackupInfoDto
            {
                FileName = fileInfo.Name,
                FilePath = fileInfo.FullName,
                FileSizeBytes = fileInfo.Length,
                CreatedAt = fileInfo.CreationTimeUtc
            };

            if (File.Exists(metadataPath))
            {
                var metadataJson = await File.ReadAllTextAsync(metadataPath);
                var metadata = JsonSerializer.Deserialize<BackupMetadata>(metadataJson);

                if (metadata != null)
                {
                    dto.CreatedAt = metadata.CreatedAt;
                    dto.HashSha256 = metadata.HashSha256;
                    dto.CompanyName = metadata.CompanyName;
                    dto.CreatedByUser = metadata.CreatedByUser;
                }
            }

            return dto;
        }
        catch
        {
            return null;
        }
    }

    private class BackupMetadata
    {
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string HashSha256 { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string OriginalDbPath { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CreatedByUser { get; set; } = string.Empty;
    }
}
