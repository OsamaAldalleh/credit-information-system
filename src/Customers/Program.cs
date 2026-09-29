using Common.Api;
using Customers.Data;
using Customers.Services;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<CustomersDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("CustomersDb"))
    .UseSnakeCaseNamingConvention()
    .UseExceptionProcessor()
    .UseSeeding((context, _) =>
    {
        if (builder.Configuration.GetValue<bool>("SeedData:Enabled"))
        {
            CustomersSeeder.Seed((CustomersDbContext)context);
        }
    }));

builder.Services.AddCommonApi();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<CustomersService>();

var app = builder.Build();

app.UseCommonApi();

// Apply pending migrations on startup so `docker compose up` needs no manual DB step.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<CustomersDbContext>().Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
