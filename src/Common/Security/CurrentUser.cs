using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Common.Security;

// The gateway validates the JWT before forwarding it, so services only read its claims.
// No token means a call from inside the system (a consumer or a job), which is not scoped to a user.
public class CurrentUser(IHttpContextAccessor httpContextAccessor)
{
    private const string BureauInstitutionType = "BUREAU";

    private readonly Lazy<JsonWebToken?> token = new(() => ReadToken(httpContextAccessor.HttpContext));

    public bool IsAuthenticated => token.Value is not null;
    public Guid? UserId => Guid.TryParse(Claim(ClaimNames.UserId), out var id) ? id : null;
    public string? Username => Claim(ClaimNames.Username);
    public Guid? InstitutionId => Guid.TryParse(Claim(ClaimNames.InstitutionId), out var id) ? id : null;
    public string? InstitutionName => Claim(ClaimNames.InstitutionName);
    public bool IsBureau => Claim(ClaimNames.InstitutionType) == BureauInstitutionType;

    // Bank users are limited to their own institution's data; bureau users and internal calls are not.
    public bool IsScopedToInstitution => IsAuthenticated && !IsBureau;

    private string? Claim(string type) => token.Value?.Claims.FirstOrDefault(c => c.Type == type)?.Value;

    private static JsonWebToken? ReadToken(HttpContext? httpContext)
    {
        var header = httpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        try
        {
            return new JsonWebTokenHandler().ReadJsonWebToken(header["Bearer ".Length..].Trim());
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
