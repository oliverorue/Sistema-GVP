namespace SistemaGVP.Application.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    string UserName { get; }
    int CompanyId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    void SetCurrentUser(Domain.Entities.User user);
    void Clear();
}