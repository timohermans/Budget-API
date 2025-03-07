using System.Reflection;
using Budget.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Budget.Application;

public static class StartupExtensions
{
    public static void AddBudgetApplication(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var config = builder.Configuration;
        
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.Name.EndsWith("UseCase"))
            .ToList()
            .ForEach(type => services.AddScoped(type));

        var fileSettings = new FileStorageSettings();
        config.GetSection("FileStorage").Bind(fileSettings);
        
        if (!fileSettings.IsValid) throw new InvalidOperationException("FileSettings are not valid");
        
        services.AddSingleton(fileSettings);
    }
}