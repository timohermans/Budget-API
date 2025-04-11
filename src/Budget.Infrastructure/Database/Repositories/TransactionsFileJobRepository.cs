using Budget.Domain.Entities;
using Budget.Domain.Repositories;

namespace Budget.Infrastructure.Database.Repositories;

public class TransactionsFileJobRepository(BudgetDbContext db) : ITransactionsFileJobRepository
{
    public async Task<TransactionsFileJob?> GetByIdAsync(Guid id)
    {
        return await db.TransactionsFileJobs.FindAsync(id);
    }

    public async Task AddAsync(TransactionsFileJob job)
    {
        await db.TransactionsFileJobs.AddAsync(job);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await db.SaveChangesAsync(cancellationToken);
    }
}