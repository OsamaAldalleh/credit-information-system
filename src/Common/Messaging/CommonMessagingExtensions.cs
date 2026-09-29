using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Messaging;

public static class CommonMessagingExtensions
{
    public static IServiceCollection AddCommonMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator> configure)
    {
        services.AddMassTransit(x =>
        {
            configure(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(
                    configuration["RabbitMq:Host"] ?? throw new InvalidOperationException("RabbitMq:Host is not configured"),
                    configuration.GetValue<ushort?>("RabbitMq:Port") ?? 5672,
                    "/",
                    h =>
                {
                    h.Username(configuration["RabbitMq:Username"]!);
                    h.Password(configuration["RabbitMq:Password"]!);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
