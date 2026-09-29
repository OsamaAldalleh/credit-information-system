using Auth.Data;
using Auth.Errors;
using Auth.Mappers;
using Auth.Models;
using Auth.Payloads;
using Common.Exceptions;
using Common.Security;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services;

public class UsersService(AuthDbContext authDb, IPasswordHasher<User> passwordHasher, CurrentUser currentUser)
{
    public async Task<UserPayload> CreateUserAsync(CreateUserPayload payload)
    {
        var institution = await authDb.Institutions.FindAsync(payload.InstitutionId!.Value)
            ?? throw ServiceException.BadRequest(AuthErrors.InstitutionNotFound, payload.InstitutionId);

        var roleNames = payload.Roles!.Distinct().ToList();
        var roles = await authDb.Roles.Where(r => roleNames.Contains(r.Name)).ToListAsync();
        var unknownRoles = roleNames.Except(roles.Select(r => r.Name)).ToList();
        if (unknownRoles.Count > 0)
        {
            throw ServiceException.BadRequest(AuthErrors.UnknownRoles, string.Join(", ", unknownRoles));
        }
        var mismatchedRole = roles.FirstOrDefault(r => r.InstitutionType != institution.Type);
        if (mismatchedRole is not null)
        {
            throw ServiceException.BadRequest(AuthErrors.RoleNotAllowedForInstitution, mismatchedRole.Name, institution.Type.ToString().ToUpperInvariant());
        }

        var now = DateTimeOffset.UtcNow;
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Username = payload.Username!.Trim().ToLowerInvariant(),
            PasswordHash = string.Empty,
            InstitutionId = institution.Id,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        user.PasswordHash = passwordHasher.HashPassword(user, payload.Password!);

        authDb.Users.Add(user);
        authDb.UserRoles.AddRange(roles.Select(r => new UserRole
        {
            UserId = user.Id,
            RoleId = r.Id,
            AssignedBy = currentUser.UserId,
            AssignedAt = now
        }));

        try
        {
            await authDb.SaveChangesAsync();
        }
        catch (UniqueConstraintException)
        {
            throw ServiceException.BadRequest(AuthErrors.UsernameTaken, user.Username);
        }

        return user.ToPayload(roles);
    }

    public async Task<UserPayload> UpdateUserRolesAsync(Guid userId, UpdateUserRolesPayload payload)
    {
        var user = await authDb.Users.FindAsync(userId)
            ?? throw ServiceException.NotFound(AuthErrors.UserNotFound, userId);
        var institution = await authDb.Institutions.FirstAsync(i => i.Id == user.InstitutionId);

        var roleNames = payload.Roles!.Distinct().ToList();
        var roles = await authDb.Roles.Where(r => roleNames.Contains(r.Name)).ToListAsync();
        var unknownRoles = roleNames.Except(roles.Select(r => r.Name)).ToList();
        if (unknownRoles.Count > 0)
        {
            throw ServiceException.BadRequest(AuthErrors.UnknownRoles, string.Join(", ", unknownRoles));
        }
        var mismatchedRole = roles.FirstOrDefault(r => r.InstitutionType != institution.Type);
        if (mismatchedRole is not null)
        {
            throw ServiceException.BadRequest(AuthErrors.RoleNotAllowedForInstitution, mismatchedRole.Name, institution.Type.ToString().ToUpperInvariant());
        }

        var now = DateTimeOffset.UtcNow;

        // The delete runs straight in the database (EF can't track a removed and a re-added row with the same key
        // in one save), so both steps share a transaction.
        await using var transaction = await authDb.Database.BeginTransactionAsync();
        await authDb.UserRoles.Where(ur => ur.UserId == userId).ExecuteDeleteAsync();
        authDb.UserRoles.AddRange(roles.Select(r => new UserRole
        {
            UserId = userId,
            RoleId = r.Id,
            AssignedBy = currentUser.UserId,
            AssignedAt = now
        }));
        user.UpdatedAt = now;

        await authDb.SaveChangesAsync();
        await transaction.CommitAsync();

        return user.ToPayload(roles);
    }
}
