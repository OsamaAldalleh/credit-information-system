using Common.Security;
using Microsoft.AspNetCore.Authorization;

namespace Gateway.Routing;

public static class GatewayPolicies
{
    public const string Anonymous = "anonymous";
    public const string Everyone = "everyone";
    public const string CustomerReaders = "customer-readers";
    public const string Writers = "writers";
    public const string BankOfficers = "bank-officers";
    public const string Bureau = "bureau";
    public const string BureauAdmin = "bureau-admin";
    public const string LoginRateLimit = "login";

    public static void AddRolePolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(Everyone, p => p.RequireRole(RoleNames.BureauAdmin, RoleNames.BureauAnalyst, RoleNames.BankOfficer, RoleNames.BankViewer));
        options.AddPolicy(CustomerReaders, p => p.RequireRole(RoleNames.BureauAdmin, RoleNames.BureauAnalyst, RoleNames.BankOfficer));
        options.AddPolicy(Writers, p => p.RequireRole(RoleNames.BureauAdmin, RoleNames.BankOfficer));
        options.AddPolicy(BankOfficers, p => p.RequireRole(RoleNames.BankOfficer));
        options.AddPolicy(Bureau, p => p.RequireRole(RoleNames.BureauAdmin, RoleNames.BureauAnalyst));
        options.AddPolicy(BureauAdmin, p => p.RequireRole(RoleNames.BureauAdmin));
    }
}
