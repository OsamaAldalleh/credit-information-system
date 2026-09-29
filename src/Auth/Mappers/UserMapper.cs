using Auth.Models;
using Auth.Payloads;

namespace Auth.Mappers;

public static class UserMapper
{
    public static UserPayload ToPayload(this User user, IEnumerable<Role> roles) => new()
    {
        Id = user.Id,
        Username = user.Username,
        InstitutionId = user.InstitutionId,
        Roles = roles.Select(r => r.Name).Order().ToList(),
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
