using Budget.Infrastructure;
using Budget.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

try
{
    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();
    var services = new ServiceCollection();

    services.AddInfrastructure(config);

    using var scope = services.BuildServiceProvider().CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();
    await context.Database.MigrateAsync();
    Console.WriteLine("Migrations applied successfully");

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Migration failed: {ex.Message}");
    return 1; // Kubernetes will detect non-zero exit as failure
}