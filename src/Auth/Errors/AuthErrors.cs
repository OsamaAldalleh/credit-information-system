using Common.Exceptions.Errors;

namespace Auth.Errors;

public static class AuthErrors
{
    public static readonly ServiceError InvalidCredentials = new("AUTH-1000", "Invalid username or password");
    public static readonly ServiceError UsernameTaken = new("AUTH-1001", "Username {0} is already taken");
    public static readonly ServiceError InstitutionNotFound = new("AUTH-1002", "Institution {0} is not found");
    public static readonly ServiceError UnknownRoles = new("AUTH-1003", "Unknown roles: {0}");
    public static readonly ServiceError RoleNotAllowedForInstitution = new("AUTH-1004", "Role {0} cannot be given to a {1} user");
    public static readonly ServiceError UserNotFound = new("AUTH-1005", "User {0} is not found");
    public static readonly ServiceError InstitutionAlreadyExists = new("AUTH-1006", "Institution {0} already exists");
}
