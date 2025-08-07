using Microsoft.Extensions.Configuration;

namespace Budget.Infrastructure.Extensions;

public static class ConfigurationExtensions
{
    public static string GetConnectionStringFromSection(this IConfiguration configuration, string sectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        var databaseSection = configuration.GetSection(sectionName);
        var dbName = databaseSection.GetValue<string>("Name") ?? "BudgetDb";
        var dbHost = databaseSection.GetValue<string>("Host") ?? "localhost";
        var dbUser = databaseSection.GetValue<string>("User") ?? "postgres";
        var dbPassword = databaseSection.GetValue<string>("Password") ?? "password";
        var connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword}";
        return connectionString;
    }
}