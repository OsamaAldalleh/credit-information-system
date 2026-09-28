using Common.Api;
using EntityFramework.Exceptions.PostgreSQL;
using Litigations.Clients.Loans;
using Litigations.Data;
using Litigations.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LitigationsDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("LitigationsDb"))
    .UseSnakeCaseNamingConvention()
    .UseExceptionProcessor());

builder.Services.AddCommonApi();
builder.Services.AddOpenApi();

builder.Services.AddScoped<LitigationsService>();

builder.Services.AddHttpClient<LoansClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Loans:BaseUrl"]
        ?? throw new InvalidOperationException("Services:Loans:BaseUrl is not configured"));
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

app.UseCommonApi();

// Apply pending migrations on startup so `docker compose up` needs no manual DB step.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<LitigationsDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
