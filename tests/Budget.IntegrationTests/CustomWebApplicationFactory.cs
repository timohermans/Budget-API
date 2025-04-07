using System.Data.Common;
using Budget.Application.Settings;
using Budget.Infrastructure.Database;
using Budget.IntegrationTests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(CustomWebApplicationFactory<Program>))]

namespace Budget.IntegrationTests;

// TODO: Implement WebApplicationFactory for real integrations tests

/// <summary>
/// Usage of the factory: https://github.com/dotnet/AspNetCore.Docs.Samples/blob/main/test/integration-tests/9.x/IntegrationTestsSample/tests/RazorPagesProject.Tests/IntegrationTests/IndexPageTests.cs
/// </summary>
/// <typeparam name="TProgram"></typeparam>
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    public FileStorageSettings FileStorageSettings { get; }
    public Action<BudgetDbContext>? DbInitAction;

    public BudgetDbContext CreateContext()
    {
        var db = new BudgetDbContext(
            new DbContextOptionsBuilder<BudgetDbContext>()
                .UseNpgsql(_postgreSqlContainer.GetConnectionString())
                .Options);
        return db;
    }


    public CustomWebApplicationFactory()
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

            services.AddScoped(_ =>
            {
                var db = CreateContext();
                db.Database.BeginTransaction();
                DbInitAction?.Invoke(db);
                return db;
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

    public async ValueTask DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}
