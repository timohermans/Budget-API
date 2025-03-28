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

    public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateOnly startDate, DateOnly endDate, string? iban)
    {
        var query = db.Transactions.AsQueryable();

        query = query
            .Where(t => t.DateTransaction >= startDate && t.DateTransaction <= endDate)
            .OrderByDescending(t => t.DateTransaction);

        if (!string.IsNullOrEmpty(iban))
        {
            query = query.Where(t => t.Iban == iban);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<string>> GetAllDistinctIbansAsync()
    {
        return await db.Transactions
            .GroupBy(t => t.Iban)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .ToListAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<CashflowDto> GetCashFlowPerIbanAsync(DateOnly startDate, DateOnly endDate, string? iban)
    {
        string? ibanForCashflow = iban;

        if (string.IsNullOrEmpty(ibanForCashflow))
        {
            ibanForCashflow = await db
                .Transactions
                .Where(t => t.DateTransaction >= startDate && t.DateTransaction <= endDate)
                .GroupBy(t => t.Iban)
                .OrderByDescending(g => g.Max(it => it.BalanceAfterTransaction))
                .Select(g => g.Any() ? g.First().Iban : null)
                .FirstOrDefaultAsync();
        }

        if (ibanForCashflow == null)
        {
            return new CashflowDto
            {
                Iban = "None",
                BalancesPerDate = []
            };
        }

        var balancesPerDate = await db
            .Transactions
            .Where(t => t.Iban == ibanForCashflow && t.DateTransaction >= startDate && t.DateTransaction <= endDate)
            .GroupBy(t => t.DateTransaction)
            .Select(g => new BalanceAtDateDto
            {
                Date = g.Key,
                Balance = g.OrderByDescending(t => t.FollowNumber).First().BalanceAfterTransaction
            })
            .OrderBy(t => t.Date)
            .ToListAsync();

        return new CashflowDto
        {
            Iban = ibanForCashflow,
            BalancesPerDate = balancesPerDate
        };
    }
}