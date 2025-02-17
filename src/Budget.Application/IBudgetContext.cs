using Budget.Domain;
using Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Budget.Application;

public interface IBudgetContext
{
    public DbSet<Transaction> Transactions { get; }
    public DbSet<TransactionsFileJob> TransactionsFileJobs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}