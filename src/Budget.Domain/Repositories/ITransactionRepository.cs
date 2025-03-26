using Budget.Domain.Contracts;
using Budget.Domain.Entities;

namespace Budget.Domain.Repositories;

public interface ITransactionRepository : IRepository
{
    Task<IEnumerable<TransactionIdDto>> GetIdsBetweenAsync(DateOnly firstDate, DateOnly lastDate);
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
    Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateOnly startDate, DateOnly endDate, string? iban);
    // ...existing code...
    Task<IEnumerable<string>> GetAllDistinctIbansAsync();
    Task<CashflowDto> GetCashFlowPerIbanAsync(DateOnly startDate, DateOnly endDate, string? iban);
}