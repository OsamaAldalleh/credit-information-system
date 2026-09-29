using Common.Api;
using Loans.Base.Services;
using Loans.Common.Clients.Customers;
using EntityFramework.Exceptions.PostgreSQL;
using Loans.Common.Data;
using Microsoft.EntityFrameworkCore;
using Loans.Payments.Services;
using Loans.Base.Clients;
using Common.Messaging;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LoansDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("LoansDb"))
    .UseSnakeCaseNamingConvention()
    .UseExceptionProcessor()
    .UseSeeding((context, _) =>
    {
        if (builder.Configuration.GetValue<bool>("SeedData:Enabled"))
        {
            LoansSeeder.Seed((LoansDbContext)context);
        }
    }));

builder.Services.AddHttpClient<LitigationsClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Litigations:BaseUrl"]
        ?? throw new InvalidOperationException("Services:Litigations:BaseUrl is not configured"));
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<PaymentsService>();
builder.Services.AddCommonApi();
builder.Services.AddCommonOpenApi();

builder.Services.AddScoped<LoansService>();

builder.Services.AddCommonMessaging(builder.Configuration, x =>
{
    x.AddEntityFrameworkOutbox<LoansDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
});

builder.Services.AddHttpClient<CustomersClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Customers:BaseUrl"]
        ?? throw new InvalidOperationException("Services:Customers:BaseUrl is not configured"));
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

app.UseCommonApi();

// Apply pending migrations on startup so `docker compose up` needs no manual DB step.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<LoansDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
