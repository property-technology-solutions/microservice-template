using System.Security.Claims;
using BuildingBlocks.Infrastructure.Security;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.API.Services;

/// <summary>
/// Default implementation of ICurrentUserService.
/// Extracts user information from HTTP context JWT claims.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

    /// <inheritdoc />
    public int? SSId
    {
        get
        {
            var ssIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue("SSId")
                         ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("ssid");
            return int.TryParse(ssIdClaim, out var ssId) ? ssId : null;
        }
    }

    /// <inheritdoc />
    public string? Role => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role)
                        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("role");

    /// <inheritdoc />
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc />
    public bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;

    /// <inheritdoc />
    public string? GetClaim(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
    }
}

