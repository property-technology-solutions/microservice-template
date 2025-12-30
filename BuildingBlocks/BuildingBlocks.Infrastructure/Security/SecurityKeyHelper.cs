using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Helper for creating JWT security keys
/// </summary>
public static class SecurityKeyHelper
{
    /// <summary>
    /// Create symmetric security key from string
    /// Used for JWT token validation
    /// </summary>
    public static SecurityKey CreateSecurityKey(string securityKey)
    {
        if (string.IsNullOrWhiteSpace(securityKey))
            throw new ArgumentException("Security key cannot be empty", nameof(securityKey));

        if (securityKey.Length < 32)
            throw new ArgumentException("Security key must be at least 32 characters", nameof(securityKey));

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
    }
}

