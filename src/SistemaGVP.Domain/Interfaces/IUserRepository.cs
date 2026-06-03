using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, int companyId);
    Task<User?> GetByEmailAsync(string email, int companyId);
}
