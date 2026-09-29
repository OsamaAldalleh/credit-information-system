namespace Auth.Models;

public class Role
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public InstitutionType InstitutionType { get; set; }
    public required string Description { get; set; }
}
