namespace Auth.Models;

public class Institution
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public InstitutionType Type { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
