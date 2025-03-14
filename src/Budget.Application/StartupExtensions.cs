using System.Reflection;
using Budget.Application.Interfaces;
using Budget.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Budget.Application;

public static class StartupExtensions
{
    public static void AddBudgetApplication(this IServiceCollection services, IConfiguration config)
    {
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith("UseCase"))
            .ToList()
            .ForEach(type =>
            {
                var interfaceType = type.GetInterfaces().FirstOrDefault();
                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, type);
                }
            });

        var fileSettings = new FileStorageSettings();
        config.GetSection("FileStorage").Bind(fileSettings);

        if (!fileSettings.IsValid) throw new InvalidOperationException("FileSettings are not valid");

        services.AddSingleton(fileSettings);
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton(TimeProvider.System);
    }
}