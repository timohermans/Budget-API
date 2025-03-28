using System.Reflection;
using Budget.Infrastructure.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Budget.Infrastructure;

public static class StartupExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration, Action<IBusRegistrationConfigurator>? configureMassTransit = null)
    {
        services.AddDbContext<BudgetContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
        });

        services.AddMassTransit(x =>
        {
            configureMassTransit?.Invoke(x);
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetValue<string>("MessageBus:Host"), "/", h =>
                {
                    h.Username(configuration.GetValue<string>("MessageBus:Username") ?? "guest");
                    h.Password(configuration.GetValue<string>("MessageBus:Password") ?? "guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith("Repository"))
            .ToList()
            .ForEach(type =>
            {
                var interfaceType = type.GetInterfaces().FirstOrDefault();
                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, type);
                }
            });
    }
}