using System.Reflection;
using Budget.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Budget.Application;

public static class ServiceCollectionExtensions
{
    public static void AddBudgetApplication(this IServiceCollection services, IConfiguration config)
    {
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.Name.EndsWith("UseCase"))
            .ToList()
            .ForEach(type => services.AddScoped(type));

        var fileSettings = new FileValidationSettings();
        config.GetSection("FileValidation").Bind(fileSettings);
        services.AddSingleton(fileSettings);
    }
}