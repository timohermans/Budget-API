using Budget.Domain.Contracts;
using Budget.Domain.Entities;

namespace Budget.Domain.Repositories;

public interface ITransactionRepository : IRepository
{
    Task<IEnumerable<TransactionIdDto>> GetIdsBetweenAsync(DateOnly firstDate, DateOnly lastDate);
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
    Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateOnly startDate, DateOnly endDate, string? iban);
    Task<IEnumerable<string>> GetAllDistinctIbansAsync();
    Task<CashflowDto> GetCashFlowPerIbanAsync(DateOnly startDate, DateOnly endDate, string? iban);
    Task<Transaction?> GetByIdAsync(int id);
    /// <summary>
    /// Not sure how useful this method is, as EF doesn't need to explicitly call update, but hey, it's here.
    /// </summary>
    Transaction Update(Transaction transaction);
}