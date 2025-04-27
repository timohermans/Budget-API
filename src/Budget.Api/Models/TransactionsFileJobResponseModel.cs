using Budget.Domain.Entities;

namespace Budget.Api.Models;

public class TransactionsFileJobResponseModel
{
    public Guid Id { get; set; }
    public string? OriginalFileName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }

    public TransactionsFileJobResponseModel() { }

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
