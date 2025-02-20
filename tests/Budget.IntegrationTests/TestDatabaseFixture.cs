using Budget.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Budget.IntegrationTests;

#region TestDatabaseFixture

public class TestDatabaseFixture
{
    private const string ConnectionString =
        @"Host=localhost;Database=Budget_test;Username=postgres;Password=P@ssw0rd;Trust Server Certificate=true;";

    private static readonly Lock Lock = new();
    private static bool _databaseInitialized;

    public TestDatabaseFixture()
    {
        lock (Lock)
        {
            if (!_databaseInitialized)
            {
                using (var context = CreateContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    // context.AddRange(
                    //     new Blog { Name = "Blog1", Url = "http://blog1.com" },
                    //     new Blog { Name = "Blog2", Url = "http://blog2.com" });
                    // context.SaveChanges();
                }

                _databaseInitialized = true;
            }
        }
    }

    public BudgetContext CreateContext()
    {
        var db = new BudgetContext(
            new DbContextOptionsBuilder<BudgetContext>()
                .UseNpgsql(ConnectionString)
                .Options);
        return db;
    }
}

#endregion