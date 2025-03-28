namespace Budget.Api.Models
{
    public class TransactionResponseModel
    {
        public int Id { get; set; }
        public int FollowNumber { get; set; }
        public required string Iban { get; set; }
        public decimal Amount { get; set; }
        public DateOnly DateTransaction { get; set; }
        public string? NameOtherParty { get; set; }
        public string? IbanOtherParty { get; set; }
        public string? AuthorizationCode { get; set; }
        public string? Description { get; set; }
        public DateOnly? CashbackForDate { get; set; }
    }
}
