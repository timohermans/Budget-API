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
        await db.Transactions.AddRangeAsync(transactions);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByAsync(int year, int month, string? iban)
    {
        var query = db.Transactions.AsQueryable();

        query = query.Where(t => t.DateTransaction.Year == year && t.DateTransaction.Month == month);

        if (!string.IsNullOrEmpty(iban))
        {
            query = query.Where(t => t.Iban == iban);
        }

        return await query.ToListAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await db.SaveChangesAsync(cancellationToken);
    }
}