using System.Security.Cryptography;
using System.Threading.RateLimiting;
using Common.Api;
using Common.Security;
using Gateway.Routing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCommonApi();

var publicKeyPath = builder.Configuration["Jwt:PublicKeyPath"]
    ?? throw new InvalidOperationException("Jwt:PublicKeyPath is not configured");
var rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText(Path.Combine(builder.Environment.ContentRootPath, publicKeyPath)));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new RsaSecurityKey(rsa),
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            NameClaimType = ClaimNames.Username,
            RoleClaimType = ClaimNames.Role,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options => options.AddRolePolicies());

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(GatewayPolicies.LoginRateLimit, context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddReverseProxy().LoadFromMemory(GatewayRoutes.Routes, GatewayRoutes.Clusters(builder.Configuration));

var app = builder.Build();

app.UseCommonApi();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        foreach (var (clusterId, name) in GatewayRoutes.ApiDocuments)
        {
            options.SwaggerEndpoint($"/openapi/{clusterId}.json", name);
        }
        options.EnablePersistAuthorization();
    });
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();
