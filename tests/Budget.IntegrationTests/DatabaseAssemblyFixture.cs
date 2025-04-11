using Budget.Application.Settings;
using Budget.Infrastructure.Database;
using Budget.IntegrationTests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(DatabaseAssemblyFixture))]

namespace Budget.IntegrationTests;

public class Sut(BudgetDbContext Db, HttpClient Client, ServiceProvider Services) : IDisposable
{
    public void Dispose()
    {
        Services.Dispose();
        Db.Dispose();
        Client.Dispose();
    }
}

public class DatabaseAssemblyFixture : IAsyncLifetime 
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    public string? ConnectionString => _postgreSqlContainer.GetConnectionString();
    public FileStorageSettings FileStorageSettings { get; private set; }

    public BudgetDbContext CreateContext()
    {
        var db = new BudgetDbContext(
            new DbContextOptionsBuilder<BudgetDbContext>()
                .UseNpgsql(_postgreSqlContainer.GetConnectionString())
                .Options);
        return db;
    }

    public DatabaseAssemblyFixture()
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

    public Task SeedDataAsync(BudgetDbContext context)
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
