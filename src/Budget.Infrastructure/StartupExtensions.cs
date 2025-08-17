using Budget.Infrastructure.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Budget.Infrastructure.Extensions;

namespace Budget.Infrastructure;

public static class StartupExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration, Action<IBusRegistrationConfigurator>? configureMassTransit = null)
    {
        services.AddDbContext<BudgetDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionStringFromSection("Database"));
        });

        services.AddMassTransit(x =>
        {
            configureMassTransit?.Invoke(x);
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetRabbitMqConnectionString("MessageBus"));
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