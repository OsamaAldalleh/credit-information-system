namespace Auth.Payloads;

public class UserPayload
{
    public Guid? Id { get; set; }
    public string? Username { get; set; }
    public Guid? InstitutionId { get; set; }
    public IReadOnlyList<string>? Roles { get; set; }
    public bool? IsActive { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}
