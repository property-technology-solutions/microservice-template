using BuildingBlocks.Infrastructure.Security;
using System.Security.Claims;

namespace HakuService.API.Services;

/// <summary>
/// Implementation of ICurrentUserService
/// Extracts user information from HTTP context claims
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public int? SSId
    {
        get
        {
            var ssIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue("SSId");
            return int.TryParse(ssIdClaim, out var ssId) ? ssId : null;
        }
    }

    public string? Role => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;

    public string? GetClaim(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
    }
}

