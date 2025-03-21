using Budget.Application.Settings;
using Budget.Infrastructure.Database;
using Budget.IntegrationTests;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(TestDatabaseFixture))]

namespace Budget.IntegrationTests;


public class TestDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    public FileStorageSettings FileStorageSettings { get; }

    public BudgetContext CreateContext()
    {
        var db = new BudgetContext(
            new DbContextOptionsBuilder<BudgetContext>()
                .UseNpgsql(_postgreSqlContainer.GetConnectionString())
                .Options);
        return db;
    }

    public TestDatabaseFixture()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json")
            .Build();

        FileStorageSettings = configuration.GetSection("FileStorage").Get<FileStorageSettings>() ?? throw new InvalidOperationException();
    }

    public async ValueTask InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        await using (var context = CreateContext())
        {
            await context.Database.EnsureCreatedAsync();
            await SeedDataAsync(context);
            await context.SaveChangesAsync();
        }
    }

    public Task SeedDataAsync(BudgetContext context)
    {
        // Fill when necessary
        // context.AddRange(
        //     new Blog { Name = "Blog1", Url = "http://blog1.com" },
        //     new Blog { Name = "Blog2", Url = "http://blog2.com" });
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}
