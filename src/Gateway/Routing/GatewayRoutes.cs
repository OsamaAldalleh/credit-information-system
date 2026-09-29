using Yarp.ReverseProxy.Configuration;
using static Gateway.Routing.GatewayPolicies;

namespace Gateway.Routing;

// Every public endpoint and the roles allowed to call it. Internal endpoints (loans credit-summary,
// litigations legal-loans/ids) are deliberately absent, so they cannot be reached through the gateway.
public static class GatewayRoutes
{
    public static IReadOnlyList<RouteConfig> Routes { get; } =
    [
        Route("login", "auth", "POST", "/api/auth/login", Anonymous, LoginRateLimit),
        Route("create-user", "auth", "POST", "/api/users", BureauAdmin),
        Route("update-user-roles", "auth", "PUT", "/api/users/{userId}/roles", BureauAdmin),
        Route("create-institution", "auth", "POST", "/api/institutions", BureauAdmin),

        Route("create-customer", "customers", "POST", "/api/customers", Writers),
        Route("get-customer", "customers", "GET", "/api/customers/{civilId}", CustomerReaders),
        Route("update-loan-eligibility", "customers", "PATCH", "/api/customers/{civilId}/loan-eligibility", BureauAdmin),
        Route("loan-eligibility-history", "customers", "GET", "/api/customers/{civilId}/loan-eligibility/history", Bureau),

        Route("create-loan", "loans", "POST", "/api/loans", Writers),
        Route("close-loan", "loans", "PATCH", "/api/loans/{loanId}/close", Writers),
        Route("get-loans", "loans", "GET", "/api/loans/{loanIdOrCivilId}", Everyone),
        Route("customer-loans-total", "loans", "GET", "/api/loans/{civilId}/total", Everyone),
        Route("delinquent-loans", "loans", "GET", "/api/loans/{civilId}/delinquent", Everyone),
        Route("next-payment", "loans", "GET", "/api/loans/{civilId}/next-payment", Everyone),

        Route("upload-payments", "loans", "POST", "/api/payments/{loanId}", Writers),
        Route("read-payments", "loans", "GET", "/api/payments/{**path}", Everyone),

        Route("create-litigation", "litigations", "POST", "/api/litigations", Writers),
        Route("update-litigation-status", "litigations", "PATCH", "/api/litigations/cases/{litigationId}/status", Writers),
        Route("get-litigation", "litigations", "GET", "/api/litigations/cases/{litigationId}", Everyone),
        Route("loan-litigations", "litigations", "GET", "/api/litigations/loan/{loanId}", Everyone),
        Route("customer-litigations", "litigations", "GET", "/api/litigations/{civilId}", Everyone),

        Route("credit-score", "credit-score", "GET", "/api/credit-scores/{civilId}", Everyone),
        Route("credit-score-history", "credit-score", "GET", "/api/credit-scores/{civilId}/history", Everyone)
    ];

    public static IReadOnlyList<ClusterConfig> Clusters(IConfiguration configuration) =>
    [
        Cluster("auth", configuration["Services:Auth:BaseUrl"]),
        Cluster("customers", configuration["Services:Customers:BaseUrl"]),
        Cluster("loans", configuration["Services:Loans:BaseUrl"]),
        Cluster("litigations", configuration["Services:Litigations:BaseUrl"]),
        Cluster("credit-score", configuration["Services:CreditScore:BaseUrl"])
    ];

    private static RouteConfig Route(string id, string clusterId, string method, string path, string policy, string? rateLimiterPolicy = null) => new()
    {
        RouteId = id,
        ClusterId = clusterId,
        AuthorizationPolicy = policy,
        RateLimiterPolicy = rateLimiterPolicy,
        Match = new RouteMatch { Path = path, Methods = [method] }
    };

    private static ClusterConfig Cluster(string id, string? address) => new()
    {
        ClusterId = id,
        Destinations = new Dictionary<string, DestinationConfig>
        {
            ["default"] = new() { Address = address ?? throw new InvalidOperationException($"The {id} service URL is not configured") }
        }
    };
}
