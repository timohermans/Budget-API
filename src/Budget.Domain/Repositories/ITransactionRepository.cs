using Budget.Domain.Contracts;
using Budget.Domain.Entities;

namespace Budget.Domain.Repositories;

public interface ITransactionRepository : IRepository
{
    Task<IEnumerable<TransactionIdDto>> GetIdsBetweenAsync(DateOnly firstDate, DateOnly lastDate);
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
}