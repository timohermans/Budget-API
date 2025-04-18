using System.Reflection;
using Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Budget.Infrastructure.Database;

public class BudgetDbContext(DbContextOptions<BudgetDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionsFileJob> TransactionsFileJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}