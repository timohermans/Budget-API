using Budget.Domain.Entities;
using Budget.Domain.Enums;

namespace Budget.Domain;

public class TransactionsFileJob
{
    public int Id { get; set; }
    public string OriginalFileName { get; set; }
    public string StoredFilePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending; // e.g., "Pending", "Processing", "Completed", "Failed"
    public string? ErrorMessage { get; set; }
}