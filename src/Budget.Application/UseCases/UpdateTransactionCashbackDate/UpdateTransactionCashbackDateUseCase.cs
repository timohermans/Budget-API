using Budget.Domain;
using Budget.Domain.Repositories;

namespace Budget.Application.UseCases.UpdateTransactionCashbackDate;

public interface IUpdateTransactionCashbackDateUseCase
{
    Task<Result<UpdateTransactionCashbackDateResponse>> HandleAsync(UpdateTransactionCashbackDateCommand command);
}

public class UpdateTransactionCashbackDateCommand
{
    public int TransactionId { get; set; }
    public DateOnly? CashbackForDate { get; set; }
}

public class UpdateTransactionCashbackDateResponse
{
    public int Id { get; set; }
    public DateOnly? CashbackForDate { get; set; }
}

public class UpdateTransactionCashbackDateUseCase : IUpdateTransactionCashbackDateUseCase
{
    private readonly ITransactionRepository _transactionRepository;

    public UpdateTransactionCashbackDateUseCase(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<UpdateTransactionCashbackDateResponse>> HandleAsync(UpdateTransactionCashbackDateCommand command)
    {
        var transaction = await _transactionRepository.GetByIdAsync(command.TransactionId);

        if (transaction == null)
        {
            return Result<UpdateTransactionCashbackDateResponse>.Failure($"Transaction with ID {command.TransactionId} not found.");
        }

        transaction.CashbackForDate = command.CashbackForDate;

        _transactionRepository.Update(transaction);
        await _transactionRepository.SaveChangesAsync();

        return Result<UpdateTransactionCashbackDateResponse>.Success(new UpdateTransactionCashbackDateResponse
        {
            Id = transaction.Id,
            CashbackForDate = transaction.CashbackForDate
        });
    }

}
