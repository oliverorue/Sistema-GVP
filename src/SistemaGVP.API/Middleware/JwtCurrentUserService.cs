using System.Security.Claims;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Entities;

namespace SistemaGVP.API.Middleware;

public class JwtCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? id : 0;
        }
    }

    public string UserName
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst("fullName")?.Value
                ?? _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value
                ?? string.Empty;
        }
    }

    public int CompanyId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirst("companyId")?.Value;
            return int.TryParse(value, out var id) ? id : 0;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin =>
        _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;

    public void SetCurrentUser(User user)
    {
    }

    public void Clear()
    {
    }
}
