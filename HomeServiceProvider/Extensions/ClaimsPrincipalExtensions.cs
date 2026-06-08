using System.Security.Claims;

namespace HomeServiceProvider.Extensions;

// Eliminates repetitive claim-parsing boilerplate across all controllers
public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim not found.");
        return Guid.Parse(claim.Value);
    }

    public static string GetRole(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Role)?.Value
           ?? throw new UnauthorizedAccessException("Role claim not found.");

    public static bool IsEmailVerified(this ClaimsPrincipal principal)
        => bool.Parse(principal.FindFirst("isEmailVerified")?.Value ?? "false");
}