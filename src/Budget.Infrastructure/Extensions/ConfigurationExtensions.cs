using Microsoft.Extensions.Configuration;
using Serilog;

namespace Budget.Infrastructure.Extensions;

public static class ConfigurationExtensions
{
    public static string GetConnectionStringFromSection(this IConfiguration configuration, string sectionName)
    {
        var aspireConnectionString = configuration.GetConnectionString("budgetdb");
        if (!string.IsNullOrWhiteSpace(aspireConnectionString))
        {
            return aspireConnectionString;
        }
        
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        var databaseSection = configuration.GetSection(sectionName);
        var dbName = databaseSection.GetValue<string>("Name") ?? "BudgetDb";
        var dbHost = databaseSection.GetValue<string>("Host") ?? "localhost";
        var dbUser = databaseSection.GetValue<string>("User") ?? "postgres";
        var dbPassword = databaseSection.GetValue<string>("Password") ?? "password";
        var connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword}";
        return connectionString;
    }
    
    public static Uri GetRabbitMqConnectionString(this IConfiguration configuration, string sectionName)
    {
        var aspireConnectionString = configuration.GetConnectionString("rabbit");
        if (!string.IsNullOrWhiteSpace(aspireConnectionString))
        {
            Log.Logger.Information(aspireConnectionString);
            return new Uri(aspireConnectionString);
        }
        
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        var section = configuration.GetSection(sectionName);
        var host = section.GetValue<string>("Host") ?? "localhost";
        var user = section.GetValue<string>("Username") ?? "guest";
        var pass = section.GetValue<string>("Password") ?? "guest";
        return new Uri($"amqp://{user}:{pass}@{host}");
    }
    
}