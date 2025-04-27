using Budget.Domain;
using Budget.Domain.Repositories;

namespace Budget.Application.UseCases.UpdateTransactionCashbackDate;

public interface IUpdateTransactionCashbackDateUseCase
{
    Task<Result<UpdateTransactionCashbackDateUseCase.Response>> HandleAsync(UpdateTransactionCashbackDateUseCase.Command command);
}

public class UpdateTransactionCashbackDateUseCase : IUpdateTransactionCashbackDateUseCase
{
    private readonly ITransactionRepository _transactionRepository;

    public UpdateTransactionCashbackDateUseCase(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<Response>> HandleAsync(Command command)
    {
        var transaction = await _transactionRepository.GetByIdAsync(command.TransactionId);

        if (transaction == null)
        {
            return Result<Response>.Failure($"Transaction with ID {command.TransactionId} not found.");
        }

        transaction.CashbackForDate = command.CashbackForDate;

        _transactionRepository.Update(transaction);
        await _transactionRepository.SaveChangesAsync();

        return Result<Response>.Success(new Response
        {
            Id = transaction.Id,
            CashbackForDate = transaction.CashbackForDate
        });
    }

    public class Command
    {
        public int TransactionId { get; set; }
        public DateOnly? CashbackForDate { get; set; }
    }

    public class Response
    {
        public int Id { get; set; }
        public DateOnly? CashbackForDate { get; set; }
    }
}
