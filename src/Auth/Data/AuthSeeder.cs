using Auth.Models;
using Common.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Data;

public static class AuthSeeder
{
    // Demo Bank A matches the institution of the seeded demo loans in the Loans service.
    private static readonly Guid BureauId = Guid.Parse("01a0ea76-0c00-7000-8000-000000000000");
    private static readonly Guid BankAId = Guid.Parse("01a0ea76-0c00-7000-8000-000000000001");
    private static readonly Guid BankBId = Guid.Parse("01a0ea76-0c00-7000-8000-000000000002");

    // Roles are reference data the system cannot work without, so they are seeded even when demo data is off.
    public static void Seed(AuthDbContext db, bool includeDemoData)
    {
        Role[] roles =
        [
            CreateRole(1, RoleNames.BureauAdmin, InstitutionType.Bureau, "Manages users and customer eligibility; full access"),
            CreateRole(2, RoleNames.BureauAnalyst, InstitutionType.Bureau, "Reads all credit information"),
            CreateRole(3, RoleNames.BankOfficer, InstitutionType.Bank, "Registers customers, loans, payments and litigations for their bank"),
            CreateRole(4, RoleNames.BankViewer, InstitutionType.Bank, "Reads loans, payments, litigations and credit scores")
        ];
        var existingRoleIds = db.Roles.AsNoTracking().Select(r => r.Id).ToHashSet();
        db.Roles.AddRange(roles.Where(r => !existingRoleIds.Contains(r.Id)));

        if (!includeDemoData)
        {
            db.SaveChanges();
            return;
        }

        Institution[] institutions =
        [
            new() { Id = BureauId, Name = "Credit Bureau", Type = InstitutionType.Bureau, CreatedAt = DateTimeOffset.UtcNow },
            new() { Id = BankAId, Name = "Demo Bank A", Type = InstitutionType.Bank, CreatedAt = DateTimeOffset.UtcNow },
            new() { Id = BankBId, Name = "Demo Bank B", Type = InstitutionType.Bank, CreatedAt = DateTimeOffset.UtcNow }
        ];
        var existingInstitutionIds = db.Institutions.AsNoTracking().Select(i => i.Id).ToHashSet();
        db.Institutions.AddRange(institutions.Where(i => !existingInstitutionIds.Contains(i.Id)));

        (User User, Role Role)[] users =
        [
            (CreateUser(1, "bureau.admin", "Bureau@Admin1", BureauId), roles[0]),
            (CreateUser(2, "bureau.analyst", "Bureau@Analyst1", BureauId), roles[1]),
            (CreateUser(3, "bank.officer", "Bank@Officer1", BankAId), roles[2]),
            (CreateUser(4, "bank.viewer", "Bank@Viewer1", BankBId), roles[3])
        ];
        var existingUserIds = db.Users.AsNoTracking().Select(u => u.Id).ToHashSet();
        foreach (var (user, role) in users.Where(u => !existingUserIds.Contains(u.User.Id)))
        {
            db.Users.Add(user);
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, AssignedAt = DateTimeOffset.UtcNow });
        }

        db.SaveChanges();
    }

    private static Role CreateRole(int number, string name, InstitutionType institutionType, string description) => new()
    {
        Id = Guid.Parse($"01a0ea76-0c00-7003-8000-{number:000000000000}"),
        Name = name,
        InstitutionType = institutionType,
        Description = description
    };

    private static User CreateUser(int number, string username, string password, Guid institutionId)
    {
        var user = new User
        {
            Id = Guid.Parse($"01a0ea76-0c00-7004-8000-{number:000000000000}"),
            Username = username,
            PasswordHash = string.Empty,
            InstitutionId = institutionId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        return user;
    }
}
