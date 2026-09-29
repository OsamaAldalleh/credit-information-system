using System.Security.Cryptography;
using Auth.Data;
using Auth.Models;
using Auth.Services;
using Common.Api;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("AuthDb"))
    .UseSnakeCaseNamingConvention()
    .UseExceptionProcessor()
    .UseSeeding((context, _) =>
        AuthSeeder.Seed((AuthDbContext)context, builder.Configuration.GetValue<bool>("SeedData:Enabled"))));

builder.Services.AddCommonApi();
builder.Services.AddCommonOpenApi();

var privateKeyPath = builder.Configuration["Jwt:PrivateKeyPath"]
    ?? throw new InvalidOperationException("Jwt:PrivateKeyPath is not configured");
var rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText(Path.Combine(builder.Environment.ContentRootPath, privateKeyPath)));
builder.Services.AddSingleton(new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<InstitutionsService>();

var app = builder.Build();

app.UseCommonApi();

// Apply pending migrations on startup so `docker compose up` needs no manual DB step.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
