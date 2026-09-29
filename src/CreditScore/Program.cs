using Common.Api;
using Common.Messaging;
using CreditScore.Clients.Customers;
using CreditScore.Clients.Litigations;
using CreditScore.Clients.Loans;
using CreditScore.Consumers;
using CreditScore.Data;
using CreditScore.Jobs;
using CreditScore.Services;
using EntityFramework.Exceptions.PostgreSQL;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CreditScoreDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("CreditScoreDb"))
    .UseSnakeCaseNamingConvention()
    .UseExceptionProcessor());

builder.Services.AddCommonApi();
builder.Services.AddOpenApi();

builder.Services.AddScoped<CreditScoresService>();

builder.Services.AddHttpClient<CustomersClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Customers:BaseUrl"]
        ?? throw new InvalidOperationException("Services:Customers:BaseUrl is not configured"));
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<LoansClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Loans:BaseUrl"]
        ?? throw new InvalidOperationException("Services:Loans:BaseUrl is not configured"));
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<LitigationsClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Litigations:BaseUrl"]
        ?? throw new InvalidOperationException("Services:Litigations:BaseUrl is not configured"));
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddCommonMessaging(builder.Configuration, x =>
{
    x.AddConsumer<CustomerCreditDataChangedConsumer>((_, cfg) =>
            cfg.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(30))))
        .Endpoint(e => e.Name = "credit-score-recalculations");
});

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey(nameof(NightlyCreditScoreJob));
    q.AddJob<NightlyCreditScoreJob>(job => job.WithIdentity(jobKey));
    q.AddTrigger(trigger => trigger
        .ForJob(jobKey)
        .WithCronSchedule(
            builder.Configuration["CreditScoreJob:Cron"] ?? "0 0 1 * * ?",
            cron => cron.InTimeZone(TimeZoneInfo.Utc)));
});
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var app = builder.Build();

app.UseCommonApi();

// Apply pending migrations on startup so `docker compose up` needs no manual DB step.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<CreditScoreDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
