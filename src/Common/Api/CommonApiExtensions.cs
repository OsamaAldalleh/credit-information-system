using System.Text.Json;
using Common.Exceptions.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Api;

public static class CommonApiExtensions
{
    public static IServiceCollection AddCommonApi(this IServiceCollection services)
    {
        services
            .AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
                // Validation errors report JSON names (civil_id) instead of C# names (CivilId).
                options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider(JsonNamingPolicy.SnakeCaseLower));
            })
            .AddJsonOptions(options => ApiJson.Configure(options.JsonSerializerOptions))
            .ConfigureApiBehaviorOptions(options =>
            {
                // ValidationFilter and StatusCodeResponseWriter produce these responses in the ApiResponse shape instead.
                options.SuppressModelStateInvalidFilter = true;
                options.SuppressMapClientErrors = true;
            });

        // The exception handlers write JSON outside MVC, which uses these separate options.
        services.ConfigureHttpJsonOptions(options => ApiJson.Configure(options.SerializerOptions));

        services.AddExceptionHandler<GlobalExceptionHandler>();
        // UseExceptionHandler() refuses to start without it; our handler writes the response, so its output is never used.
        services.AddProblemDetails();

        return services;
    }

    public static WebApplication UseCommonApi(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages(StatusCodeResponseWriter.WriteAsync);

        return app;
    }
}
