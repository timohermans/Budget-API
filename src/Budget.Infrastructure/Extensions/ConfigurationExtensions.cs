using Microsoft.Extensions.Configuration;
using Serilog;

namespace Budget.Infrastructure.Extensions;

public static class ConfigurationExtensions
{
    public static string GetConnectionStringFromSection(this IConfiguration configuration, string connectionStringName = "budgetdb")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        
        return connectionString;
    }
    
    public static Uri GetRabbitMqConnectionString(this IConfiguration configuration, string connectionStringName = "rabbit")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        
        return new Uri(connectionString);
    }
}