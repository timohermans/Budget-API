using Budget.Domain.Contracts;
using Budget.Domain.Entities;
using Budget.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Budget.Infrastructure.Database.Repositories;

public class TransactionRepository(BudgetContext db) : ITransactionRepository
{
    public async Task<IEnumerable<TransactionIdDto>> GetIdsBetweenAsync(DateOnly firstDate, DateOnly lastDate)
    {
        return await db.Transactions
            .Where(t => t.DateTransaction >= firstDate && t.DateTransaction <= lastDate)
            .Select(t => new TransactionIdDto
            {
                Id = t.Id,
                FollowNumber = t.FollowNumber,
                Iban = t.Iban
            })
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        await db.AddRangeAsync(transactions);
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await db.SaveChangesAsync(cancellationToken);
    }
}