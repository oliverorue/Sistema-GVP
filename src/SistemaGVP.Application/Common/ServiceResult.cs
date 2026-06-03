namespace SistemaGVP.Application.Common;

/// <summary>
/// Resultado genérico para operaciones de servicio.
/// Evita el uso de excepciones para flujo de control.
/// </summary>
public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public List<string> Errors { get; private set; } = new();
    public bool RequiresPasswordChange { get; private set; }

    public static ServiceResult<T> Success(T data, string message = "")
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message
        };
    }

    public static ServiceResult<T> SuccessRequiresPasswordChange(T data, string message = "")
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            RequiresPasswordChange = true
        };
    }

    public static ServiceResult<T> Failure(string message)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = new List<string> { message }
        };
    }

    public static ServiceResult<T> Failure(List<string> errors)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Message = errors.FirstOrDefault() ?? "Error",
            Errors = errors
        };
    }
}
