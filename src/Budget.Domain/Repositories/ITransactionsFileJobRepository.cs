namespace Budget.Domain.Repositories;

public interface ITransactionsFileJobRepository : IRepository
{
    Task<TransactionsFileJob?> GetByIdAsync(Guid id);
    Task AddAsync(TransactionsFileJob job);
}