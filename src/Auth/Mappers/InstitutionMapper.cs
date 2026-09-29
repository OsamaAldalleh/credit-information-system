using Auth.Models;
using Auth.Payloads;

namespace Auth.Mappers;

public static class InstitutionMapper
{
    public static InstitutionPayload ToPayload(this Institution institution) => new()
    {
        Id = institution.Id,
        Name = institution.Name,
        Type = institution.Type,
        CreatedAt = institution.CreatedAt
    };

    public static Institution ToEntity(this CreateInstitutionPayload payload) => new()
    {
        Id = Guid.CreateVersion7(),
        Name = payload.Name!.Trim(),
        Type = payload.Type!.Value,
        CreatedAt = DateTimeOffset.UtcNow
    };
}
