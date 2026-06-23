using Microsoft.EntityFrameworkCore;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Interfaces;
using SistemaGVP.Infrastructure.Data;

namespace SistemaGVP.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
    }

    public async Task<bool> UsernameExistsAsync(string username, int companyId, int? excludeUserId = null)
    {
        var query = _dbSet.Where(u => u.Username == username
                                   && u.CompanyId == companyId
                                   && u.IsActive);

        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);

        return await query.AnyAsync();
    }
}
