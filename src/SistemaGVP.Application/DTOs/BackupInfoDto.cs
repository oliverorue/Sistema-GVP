namespace SistemaGVP.Application.DTOs;

public class BackupInfoDto
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileSizeDisplay => FileSizeBytes switch
    {
        < 1024 => $"{FileSizeBytes} B",
        < 1048576 => $"{FileSizeBytes / 1024.0:F1} KB",
        _ => $"{FileSizeBytes / 1048576.0:F1} MB"
    };
    public DateTime CreatedAt { get; set; }
    public string? HashSha256 { get; set; }
    public string? CompanyName { get; set; }
    public string? CreatedByUser { get; set; }
}
