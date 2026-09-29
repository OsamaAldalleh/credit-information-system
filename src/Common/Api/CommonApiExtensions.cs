using System.Text.Json;
using Common.Exceptions.Handlers;
using Common.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

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

        services.AddHttpContextAccessor();
        services.AddScoped<CurrentUser>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        // UseExceptionHandler() refuses to start without it; our handler writes the response, so its output is never used.
        services.AddProblemDetails();

        return services;
    }

    public static IServiceCollection AddCommonOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
        {
            // The docs are served through the gateway, so "Try it out" must call the gateway, not this service.
            document.Servers = [];
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" }
            };
            document.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] }];
            return Task.CompletedTask;
        }));

        return services;
    }

    public static WebApplication UseCommonApi(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages(StatusCodeResponseWriter.WriteAsync);

        return app;
    }
}
