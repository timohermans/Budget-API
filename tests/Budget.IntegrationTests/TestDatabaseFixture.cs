using Budget.Infrastructure.Database;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Budget.IntegrationTests;

#region TestDatabaseFixture

public class TestDatabaseFixture : IAsyncLifetime
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private static bool _databaseInitialized;
    private static readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    public BudgetContext CreateContext()
    {
        var db = new BudgetContext(
            new DbContextOptionsBuilder<BudgetContext>()
                .UseNpgsql(_postgreSqlContainer.GetConnectionString())
                .Options);
        return db;
    }

    public async Task InitializeAsync()
    {
        await Semaphore.WaitAsync();
        try
        {
            if (!_databaseInitialized)
            {
                await _postgreSqlContainer.StartAsync();
                await using (var context = CreateContext())
                {
                    await context.Database.EnsureCreatedAsync();
                    await SeedDataAsync(context);
                    await context.SaveChangesAsync();
                }

                _databaseInitialized = true;
            }
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task SeedDataAsync(BudgetContext context)
    {
        // Fill when necessary
        // context.AddRange(
        //     new Blog { Name = "Blog1", Url = "http://blog1.com" },
        //     new Blog { Name = "Blog2", Url = "http://blog2.com" });
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}

#endregion