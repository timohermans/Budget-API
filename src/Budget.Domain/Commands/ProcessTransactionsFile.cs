namespace Budget.Domain.Commands;

public class ProcessTransactionsFile
{
    public Guid CommandId { get; set; }
    public int JobId { get; set; }
}