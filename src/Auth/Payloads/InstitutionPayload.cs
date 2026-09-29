using Auth.Models;

namespace Auth.Payloads;

public class InstitutionPayload
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public InstitutionType? Type { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}
