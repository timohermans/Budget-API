using System.Reflection;
using Budget.Infrastructure.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Budget.Infrastructure;

public static class StartupExtensions
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        var config = builder.Configuration;

        builder.Services.AddDbContext<BudgetContext>(options =>
        {
            options.UseNpgsql(config.GetConnectionString("Default"));
        });

        builder.Services.AddMassTransit(x =>
        {
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
        // builder.Services.AddHostedService<Worker>();

        Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith("Repository"))
            .ToList()
            .ForEach(type =>
            {
                var interfaceType = type.GetInterfaces().FirstOrDefault();
                if (interfaceType != null)
                {
                    builder.Services.AddScoped(interfaceType, type);
                }
            });
    }
}