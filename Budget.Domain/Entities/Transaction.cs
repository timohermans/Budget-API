namespace Budget.Domain.Entities;

public class Transaction
{
    public int Id { get; init; }
    public int FollowNumber { get; init; }
    public required string Iban { get; init; }
    public required string Currency { get; init; }
    public decimal Amount { get; init; }
    public DateOnly DateTransaction { get; init; }
    public decimal BalanceAfterTransaction { get; init; }
    public string? NameOtherParty { get; init; }
    public string? IbanOtherParty { get; init; }
    public string? AuthorizationCode { get; init; }
    public string? Description { get; init; }
    public DateOnly? CashbackForDate { get; init; }
}