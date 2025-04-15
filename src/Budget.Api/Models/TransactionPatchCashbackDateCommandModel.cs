namespace Budget.Api.Models;

public class TransactionPatchCashbackDateCommandModel
{
    public DateOnly? CashbackForDate { get; set; }
}
