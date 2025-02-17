using Budget.Application;
using Budget.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Budget.Infrastructure;

public static class StartupExtensions
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        var config = builder.Configuration;

        builder.Services.AddDbContext<BudgetContext>(options =>
        {
            options.UseNpgsql(config.GetConnectionString("Default"));
        });

        builder.Services.AddScoped<IBudgetContext, BudgetContext>();
    }
}