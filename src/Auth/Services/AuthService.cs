using Auth.Data;
using Auth.Errors;
using Auth.Models;
using Auth.Payloads;
using Common.Exceptions;
using Common.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Services;

public class AuthService(
    AuthDbContext authDb,
    IPasswordHasher<User> passwordHasher,
    SigningCredentials signingCredentials,
    IConfiguration configuration)
{
    public async Task<TokenPayload> LoginAsync(LoginPayload payload)
    {
        var username = payload.Username!.Trim().ToLowerInvariant();
        var user = await authDb.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);

        // Unknown user, wrong password and disabled user all get the same answer, so usernames cannot be probed.
        if (user is null
            || !user.IsActive
            || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, payload.Password!) == PasswordVerificationResult.Failed)
        {
            throw ServiceException.Unauthorized(AuthErrors.InvalidCredentials);
        }

        var institution = await authDb.Institutions.AsNoTracking().FirstAsync(i => i.Id == user.InstitutionId);
        var roles = await authDb.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(authDb.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .ToArrayAsync();

        var lifetime = TimeSpan.FromMinutes(configuration.GetValue("Jwt:LifetimeMinutes", 60));
        var now = DateTime.UtcNow;

        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            IssuedAt = now,
            NotBefore = now,
            Expires = now.Add(lifetime),
            SigningCredentials = signingCredentials,
            Claims = new Dictionary<string, object>
            {
                [ClaimNames.UserId] = user.Id.ToString(),
                [ClaimNames.Username] = user.Username,
                [ClaimNames.InstitutionId] = institution.Id.ToString(),
                [ClaimNames.InstitutionName] = institution.Name,
                [ClaimNames.InstitutionType] = institution.Type.ToString().ToUpperInvariant(),
                [ClaimNames.Role] = roles
            }
        });

        return new TokenPayload
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = (int)lifetime.TotalSeconds
        };
    }
}
