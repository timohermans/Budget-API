using Budget.Infrastructure.Database;
using Budget.IntegrationTests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data.Common;

[assembly: AssemblyFixture(typeof(DatabaseAssemblyFixture))]

namespace Budget.IntegrationTests;

/// <summary>
/// Usage of the factory: https://github.com/dotnet/AspNetCore.Docs.Samples/blob/main/test/integration-tests/9.x/IntegrationTestsSample/tests/RazorPagesProject.Tests/IntegrationTests/IndexPageTests.cs
/// </summary>
/// <typeparam name="TProgram"></typeparam>
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly string _connectionString;

    private CustomWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public static async Task<CustomWebApplicationFactory<TProgram>> CreateApiClientAsync(string? connectionString, string testName, CancellationToken? cancellationToken = null)
    {
        // postgres table is max 63 chars, without giving errors. Guid makes sure db names are unique if I'd use xunit theories
        var dbName = $"{testName.Substring(0, 27)}_{Guid.NewGuid().ToString().Replace("-", "_")}".ToLower();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken ?? CancellationToken.None);
        using var command = new NpgsqlCommand($"Create database {dbName}", connection);
        await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);

        var apiConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = dbName
        }.ToString();

        await using var context = new BudgetDbContext(
          new DbContextOptionsBuilder<BudgetDbContext>()
              .UseNpgsql(apiConnectionString)
              .Options);

        await context.Database.MigrateAsync(cancellationToken ?? CancellationToken.None);
        // future seeding
        await context.SaveChangesAsync(cancellationToken ?? CancellationToken.None);

        var factory = new CustomWebApplicationFactory<TProgram>(apiConnectionString);
        return factory;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(IDbContextOptionsConfiguration<BudgetDbContext>));

            if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbConnection));

            if (dbConnectionDescriptor != null) services.Remove(dbConnectionDescriptor);

            // Create open Postgres Connection so EF won't automatically close it.
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                return connection;
            });

            services.AddDbContext<BudgetDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseNpgsql(connection);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
             .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

        });

        builder.UseEnvironment("Development");
    }
}
