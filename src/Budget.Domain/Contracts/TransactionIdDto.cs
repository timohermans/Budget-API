namespace Budget.Domain.Contracts;

public class TransactionIdDto
{
    public required int Id { get; init; }
    public required string Iban { get; init; }
    public required int FollowNumber { get; set; }
}