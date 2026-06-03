using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Interfaces;

public interface IBackupService
{
    Task<ServiceResult<string>> CreateBackupAsync(int companyId, int userId);
    Task<ServiceResult<bool>> RestoreBackupAsync(string backupFilePath, int companyId, int userId);
    Task<ServiceResult<List<BackupInfoDto>>> GetBackupsAsync(int companyId);
    Task<ServiceResult<BackupInfoDto>> GetBackupInfoAsync(string backupFilePath);
}
