using Budget.Domain.Entities;

namespace Budget.Api.Models;

public class TransactionsFileJobResponseModel
{
    public Guid Id { get; }
    public string OriginalFileName { get; }
    public DateTime CreatedAt { get; }
    public DateTime? ProcessedAt { get; }
    public string Status { get; }
    public string? ErrorMessage { get; }

    public TransactionsFileJobResponseModel(TransactionsFileJob job)
    {
        Id = job.Id;
        OriginalFileName = job.OriginalFileName;
        CreatedAt = job.CreatedAt;
        ProcessedAt = job.ProcessedAt;
        Status = job.Status.ToString();
        ErrorMessage = job.ErrorMessage;
    }
}
