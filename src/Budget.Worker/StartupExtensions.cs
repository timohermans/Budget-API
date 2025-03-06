using System.Reflection;
using Budget.Worker.Consumers;
using MassTransit;

namespace Budget.Worker;

public static class StartupExtensions
{
    public static void AddWorker(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration;
        services.AddMassTransit(x =>
        {
            var entryAssembly = Assembly.GetEntryAssembly();

            x.AddConsumers(entryAssembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(config.GetValue<string>("MessageBus:Host"), "/", h =>
                {
                    h.Username(config.GetValue<string>("MessageBus:Username") ?? "guest");
                    h.Password(config.GetValue<string>("MessageBus:Password") ?? "guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    }
}